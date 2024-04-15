using Lua14.Systems;
using NLua;
using Robust.Client;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries;

public class LuaSystemLibrary : LuaLibrary
{
    [Dependency] private readonly LuaSystem _luaSystem = default!;
    [Dependency] private readonly NLua.Lua _lua = default!;
    [Dependency] private readonly BaseClient _baseClient = default!;

    private readonly List<LuaFunction> _starters = [];

    public override void Initialize()
    {
        _baseClient.PlayerJoinedServer += RunInit;
    }


    public override string Name => "lua_sys";

    private void RunInit(object? sender, PlayerEventArgs e)
    {
        foreach (var starter in _starters)
        {
            starter.Call();
        }
    }

    [LuaMethod("addStarter")]
    public void AddStarter(LuaFunction starterFunc)
    {
        _starters.Add(starterFunc);
    }

    [LuaMethod("addSystem")]
    public bool AddSystem(string name, LuaFunction? initFunc = null, LuaFunction? updateFunc = null)
    {
        var table = _lua.NewTable();
        return _luaSystem.TryPutLuaSystem(name, initFunc, updateFunc, table);
    }

    [LuaMethod("removeSystem")]
    public bool RemoveSystem(string name)
    {
        return _luaSystem.TryRemoveLuaSystem(name);
    }
}