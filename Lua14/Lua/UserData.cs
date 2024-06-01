using HarmonyLib;
using NLua;
using System.Reflection;

namespace Lua14.Lua;

public class UserData
{
    private static readonly MethodInfo M_Call = AccessTools.Method(typeof(LuaFunction), "Call");

    protected readonly NLua.Lua Lua;

    private readonly LuaTable _metatable;
    private readonly Dictionary<string, LuaFunction> _originalMetamethods = [];

    public UserData(NLua.Lua state)
    {
        Lua = state;
        _metatable = Lua.GetMetatable(this) ?? throw new Exception("No metatable found for a c# userdata.");

        RegisterMetaMethods();
    }

    private void RegisterMetaMethods()
    {
        RegisterMetaMethod("__index", "Index");
        RegisterMetaMethod("__newindex", "NewIndex");
        RegisterMetaMethod("__call", "Call");
        RegisterMetaMethod("__iter", "Iter");
    }

    private void RegisterMetaMethod(string metamethod, string methodName)
    {
        LuaFunction func = Lua.CreateFunction(
            GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance),
            this
        );
        LuaFunction wrappedFunc = Lua.WrapFunction(func);

        _originalMetamethods[metamethod] = (LuaFunction)_metatable[metamethod];
        _metatable[metamethod] = wrappedFunc;
    }

    /// <summary>
    /// Fires when table[index] is indexed.
    /// </summary>
    protected virtual object Index(UserData self, object index)
    {
        var __index = _originalMetamethods["__index"];
        return __index.Call(self, index);
    }

    /// <summary>
    /// Fires when table[index] tries to be set (table[index] = value).
    /// </summary>
    protected virtual void NewIndex(UserData self, object index, object value)
    {
        var __newindex = _originalMetamethods["__newindex"];
        __newindex.Call(self, index, value);
    }

    /// <summary>
    /// Fires when the table is called like a function.
    /// By default its the current object constructor.
    /// </summary>
    /// <param name="args">The arguments that were passed.</param>
    protected virtual object Call(UserData self, params object[] args)
    {
        var __call = _originalMetamethods["__call"];
        return M_Call.Invoke(__call, [self, ..args]);
    }

    /// <summary>
    /// 	Used to denote a custom iterator when using generalized iteration.
    /// </summary>
    protected virtual LuaFunction Iter(UserData self)
    {
        throw new NotImplementedException();
    }

    public object this[string index]
    {
        get {
            object result = Index(this, index);

            if (result is object[] array)
                return array[0];

            return result;
        }
        set => NewIndex(this, index, value);
    }
}
