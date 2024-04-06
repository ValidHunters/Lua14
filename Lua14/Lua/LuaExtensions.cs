using NLua;

namespace Lua14.Lua;

public static class LuaExtensions
{
    private static readonly string _tempPath = "_LUA14_TEMP";

    public static LuaTable NewTable(this NLua.Lua Lua)
    {
        Lua.NewTable(_tempPath);
        LuaTable table = Lua.GetTable(_tempPath);
        Lua[_tempPath] = null;
        return table;
    }
    public static LuaTable EnumerableToTable<T>(this NLua.Lua Lua, IEnumerable<T> enumerable)
    {
        LuaTable table = Lua.NewTable();
        List<T> enumerableList = enumerable.ToList();

        for (int i = 0; i < enumerableList.Count; i++)
        {
            table[i + 1] = enumerableList[i];
        }

        return table;
    }
    public static Dictionary<T1, T2>? TableToDictionary<T1, T2>(this NLua.Lua Lua, LuaTable table) where T1 : notnull
    {
        Dictionary<object, object> dictionary = Lua.GetTableDict(table);
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
    public static IEnumerable<T>? TableToEnumerable<T>(this NLua.Lua Lua, LuaTable table)
    {
        var dictionary = Lua.TableToDictionary<int, T>(table);
        if (dictionary == null)
            return null;

        List<T> result = [];
        foreach (var pair in dictionary)
        {
            result[pair.Key] = pair.Value;
        }

        return result;
    }
    public static LuaTable? GetMetatable(this NLua.Lua Lua, LuaTable table)
    {
        LuaFunction getmetatable = Lua.GetFunction("getmetatable");
        object[] metatable = getmetatable.Call(table);

        if (metatable.Length < 1)
            return null;

        return metatable[0] as LuaTable;
    }
    public static void SetMetatable(this NLua.Lua Lua, LuaTable table, LuaTable metatable)
    {
        LuaFunction setmetatable = Lua.GetFunction("setmetatable");
        setmetatable.Call(table, metatable);
    }
}
