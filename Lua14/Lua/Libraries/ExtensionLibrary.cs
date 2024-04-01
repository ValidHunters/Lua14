using Lua14.Data;
using NLua;

namespace Lua14.Lua.Libraries;

public sealed class ExtensionLibrary(NLua.Lua lua, LuaMod mod, LuaLogger log) : LuaLibrary(lua, mod, log)
{
    public override string Name => "c#extensions";
    public override bool IsLibraryGlobal => true;

    private static readonly string _tempPath = "_LUA14_TEMP"; 
    private LuaFunction _getmetatable = default!;
    private LuaFunction _setmetatable = default!;

    public override void Initialize()
    {
        _getmetatable = Lua.GetFunction("getmetatable");
        _setmetatable = Lua.GetFunction("setmetatable");
    }

    public LuaTable NewTable()
    {
        Lua.NewTable(_tempPath);
        LuaTable table = Lua.GetTable(_tempPath);
        Lua[_tempPath] = null;
        return table;
    }
    public LuaTable EnumerableToTable<T>(IEnumerable<T> enumerable)
    {
        LuaTable table = NewTable();
        List<T> typesList = enumerable.ToList();

        for (int i = 0; i < typesList.Count; i++)
        {
            table[i + 1] = typesList[i];
        }

        return table;
    }
    public LuaTable GetMetatable(LuaTable table)
    {
        return (LuaTable)_getmetatable.Call(table)[0];
    }
    public void SetMetatable(LuaTable table, LuaTable metatable)
    {
        _setmetatable.Call(table, metatable);
    }
}
