using NLua;
using System.Reflection;

namespace Lua14.Lua;

public static class LuaExtensions
{
    private const string TempPath = "_LUA14_TEMP";
    private const string ChunkName = "LUA14";

    private const string NativeFunctionWrapper = "local a=_LUA14_TEMP;local b=table.unpack;return function(...)local c={a(...)}return b(c)end";
    /*
    local functionUserdata = _LUA14_TEMP
    local unpack = table.unpack
    return function(...)
        local result = { functionUserdata(...) }
        return unpack(result)
    end
    */

    /// <summary>
    /// When we use a c# function from lua
    /// NLua creates a userdata with a __call and __gc metamethods.
    /// This is used to wrap that userdata in a function.
    /// </summary>
    public static LuaFunction WrapFunction(this NLua.Lua lua, LuaFunction function)
    {
        lua[TempPath] = function;
        var result = (LuaFunction)lua.DoString(NativeFunctionWrapper, ChunkName)[0];
        lua[TempPath] = null;

        return result;
    }

    public static LuaFunction CreateFunction(this NLua.Lua lua, MethodBase method, object target = null)
    {
        LuaFunction function = lua.RegisterFunction(TempPath, target, method);
        lua[TempPath] = null;

        return function;
    }

    public static LuaTable EnumerableToTable<T>(this NLua.Lua lua, IEnumerable<T> enumerable)
    {
        var table = lua.NewTable();
        int index = 0;

        foreach (var item in enumerable)
        {
            table[index++] = item;
        }

        return table;
    }

    public static Dictionary<T1, T2> TableToDictionary<T1, T2>(this NLua.Lua lua, LuaTable table) where T1 : notnull
    {
        Dictionary<object, object> dictionary = lua.GetTableDict(table);
        Dictionary<T1, T2> result = [];
        foreach (var pair in dictionary)
        {
            if (pair.Key is not T1 keyCasted || pair.Value is not T2 valueCasted)
            {
                return null;
            }
            result[keyCasted] = valueCasted;
        }

        return result;
    }

    public static IEnumerable<T> TableToEnumerable<T>(this NLua.Lua lua, LuaTable table)
    {
        var dictionary = lua.TableToDictionary<int, T>(table);
        if (dictionary == null)
            return null;

        List<T> result = [];
        foreach (var pair in dictionary)
        {
            result[pair.Key] = pair.Value;
        }

        return result;
    }

    public static LuaTable GetMetatable(this NLua.Lua lua, object target)
    {
        return lua.GetFunction("getmetatable").Call(target).FirstOrDefault() as LuaTable;
    }

    public static LuaTable SetMetatable(this NLua.Lua lua, object target, LuaTable metatable)
    {
        return (LuaTable)lua.GetFunction("setmetatable").Call(target, metatable).First();
    }
}
