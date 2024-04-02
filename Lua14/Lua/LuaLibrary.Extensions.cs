using NLua;

namespace Lua14.Lua;

public partial class LuaLibrary
{
    private static readonly string _tempPath = "_LUA14_TEMP";
    private LuaFunction _getmetatable = default!;
    private LuaFunction _setmetatable = default!;

    private void InitializeExtensions()
    {
        _getmetatable = Lua.GetFunction("getmetatable");
        _setmetatable = Lua.GetFunction("setmetatable");
    }
    protected LuaTable NewTable()
    {
        Lua.NewTable(_tempPath);
        LuaTable table = Lua.GetTable(_tempPath);
        Lua[_tempPath] = null;
        return table;
    }
    protected LuaTable EnumerableToTable<T>(IEnumerable<T> enumerable)
    {
        LuaTable table = NewTable();
        List<T> enumerableList = enumerable.ToList();

        for (int i = 0; i < enumerableList.Count; i++)
        {
            table[i + 1] = enumerableList[i];
        }

        return table;
    }
    protected Dictionary<T1, T2>? TableToDictionary<T1, T2>(LuaTable table) where T1: notnull
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
    protected IEnumerable<T>? TableToEnumerable<T>(LuaTable table)
    {
        var dictionary = TableToDictionary<int, T>(table);
        if (dictionary == null)
            return null;

        List<T> result = [];
        foreach (var pair in dictionary)
        {
            result[pair.Key] = pair.Value;
        }

        return result;
    }
    protected LuaTable GetMetatable(LuaTable table)
    {
        return (LuaTable)_getmetatable.Call(table)[0];
    }
    protected void SetMetatable(LuaTable table, LuaTable metatable)
    {
        _setmetatable.Call(table, metatable);
    }
}
