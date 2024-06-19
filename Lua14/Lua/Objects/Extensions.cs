using Eluant;
using Eluant.ObjectBinding;
using HarmonyLib;
using Lua14.Lua.Objects.CLR;
using System.Diagnostics.CodeAnalysis;

namespace Lua14.Lua.Objects;

interface ILuaBindable { }

public static class Extensions
{
    public static Type WrappedClrType(this LuaValue value)
    {
        if (value.TryGetClrObject(out var inner))
            return inner.GetType();

        return value.GetType();
    }

    public static bool TryGetClrValue<T>(this LuaValue value, [NotNullWhen(true)] out T? clrObject)
    {
        var ret = value.TryGetClrValue(typeof(T), out var temp);
        clrObject = ret ? (T)temp! : default;

        return ret;
    }

    public static bool TryGetClrValue(this LuaValue value, Type t, [NotNullWhen(true)] out object? clrObject)
    {
        // Is t a nullable?
        // If yes, get the underlying type
        var nullable = Nullable.GetUnderlyingType(t);
        if (nullable != null)
            t = nullable;

        // Value wraps a CLR object
        if (value.TryGetClrObject(out var temp) && temp.GetType() == t)
        {
            clrObject = temp;
            return true;
        }

        if (value is LuaNil && !t.IsValueType)
        {
            clrObject = default!;
            return true;
        }

        if (value is LuaBoolean && t.IsAssignableFrom(typeof(bool)))
        {
            clrObject = value.ToBoolean();
            return true;
        }

        if (value is LuaNumber number)
        {
            if (t.IsAssignableFrom(typeof(double)))
            {
                clrObject = number.Value;
                return true;
            }

            // Need an explicit test for double -> int
            // TODO: Lua 5.3 will introduce an integer type, so this will be able to go away
            if (t.IsAssignableFrom(typeof(int)))
            {
                clrObject = (int)number.Value;
                return true;
            }

            if (t.IsAssignableFrom(typeof(short)))
            {
                clrObject = (short)number.Value;
                return true;
            }

            if (t.IsAssignableFrom(typeof(byte)))
            {
                clrObject = (byte)number.Value;
                return true;
            }
        }

        if (value is LuaString && t.IsAssignableFrom(typeof(string)))
        {
            clrObject = value.ToString();
            return true;
        }

        if (value is LuaFunction && t.IsAssignableFrom(typeof(LuaFunction)))
        {
            clrObject = value;
            return true;
        }

        if (value is LuaTable && t.IsAssignableFrom(typeof(LuaTable)))
        {
            clrObject = value;
            return true;
        }

        // Translate LuaTable<int, object> -> object[]
        if (value is LuaTable table && t.IsArray)
        {
            var innerType = t.GetElementType()!;
            var array = Array.CreateInstance(innerType, table.Count);
            var i = 0;

            foreach (var kv in table)
            {
                using (kv.Key)
                {
                    object? element;
                    if (innerType == typeof(LuaValue))
                        element = kv.Value;
                    else
                    {
                        var elementHasClrValue = kv.Value.TryGetClrValue(innerType, out element);
                        if (!elementHasClrValue || element is not LuaValue)
                            kv.Value.Dispose();
                        if (!elementHasClrValue)
                            throw new LuaException($"Unable to convert table value of type {kv.Value.WrappedClrType()} to type {innerType}");
                    }

                    array.SetValue(element, i++);
                }
            }

            clrObject = array;
            return true;
        }

        // Value isn't of the requested type.
        // Set a default output value and return false
        // Value types are assumed to specify a default constructor
        clrObject = t.IsValueType ? Activator.CreateInstance(t) : null;
        return false;
    }

    public static LuaValue ToLuaValue(this object obj, LuaRuntime runtime)
    {
        if (obj is LuaValue v)
            return v;

        if (obj == null)
            return LuaNil.Instance;

        if (obj is double d)
            return d;

        if (obj is int integer)
            return integer;

        if (obj is bool b)
            return b;

        if (obj is string s)
            return s;

        if (obj is ILuaBindable)
            return new LuaCustomClrObject(obj);

        if (obj is IEnumerable<LuaValue> enumerable)
            return runtime.CreateTable(enumerable);

        if (obj is Array array)
        {
            var i = 1;
            var table = runtime.CreateTable();

            foreach (var x in array)
                using (LuaValue key = i++, value = x.ToLuaValue(runtime))
                    table.Add(key, value);

            return table;
        }

        throw new InvalidOperationException($"Cannot convert type '{obj.GetType()}' to Lua. Class must implement ILuaBindable.");
    }

    private static readonly BasicBindingSecurityPolicy _securityPolicy = new(MemberSecurityPolicy.Permit);
    private static readonly MemberNameLuaBinder _binder = new(AccessTools.all);

    public static LuaTransparentClrObject AsClrObj(this object obj) 
    {
        return new(obj, _binder, _securityPolicy);
    }

    public static LuaVararg Call(this LuaFunction function, params object[] arguments)
    {
        IList<LuaValue> args = 
            arguments
            .Select(AsClrObj)
            .Cast<LuaValue>()
            .ToList();

        LuaVararg ret = function.Call(args);

        foreach(var arg in args)
            arg.Dispose();

        return ret;
    }
}
