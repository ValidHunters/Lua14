using NLua;

namespace Lua14.Lua;

public static class LuaExtensions
{
    private const string TempPath = "_LUA14_TEMP";

    public static LuaTable NewTable(this NLua.Lua lua)
    {
        lua.NewTable(TempPath);
        var table = lua.GetTable(TempPath);
        lua[TempPath] = null;
        return table;
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

    public static Dictionary<T1, T2>? TableToDictionary<T1, T2>(this NLua.Lua lua, LuaTable table) where T1 : notnull
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

    public static IEnumerable<T>? TableToEnumerable<T>(this NLua.Lua lua, LuaTable table)
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

    public static LuaTable? GetMetatable(this NLua.Lua lua, LuaTable table)
    {
        return lua.GetFunction("getmetatable").Call(table).FirstOrDefault() as LuaTable;
    }

    public static void SetMetatable(this NLua.Lua lua, LuaTable table, LuaTable metatable)
    {
        lua.GetFunction("setmetatable").Call(table, metatable);
    }
}
