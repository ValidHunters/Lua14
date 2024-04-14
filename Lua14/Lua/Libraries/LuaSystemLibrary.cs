using HarmonyLib;
using Lua14.Systems;
using NLua;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;

namespace Lua14.Lua.Libraries;

public class LuaSystemLibrary : LuaLibrary
{
    [Dependency] private readonly HarmonyLib.Harmony _harmony = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    private LuaSystem _luaSystem = null!;
    [Dependency] private readonly NLua.Lua _lua = default!;
    [Dependency] private readonly LuaLogger _logger = default!;
    private static List<LuaFunction> _starters = [];

    public override void Initialize()
    {
        _harmony.Patch(AccessTools.Method(AccessTools.TypeByName("Robust.Client.BaseClient"), "OnPlayerJoinedServer"),
            postfix: new HarmonyMethod(Postfix));
    }

    public override string Name => "lua-sys";
    
    [LuaMethod("starter")]
    public void AddStarter(LuaFunction starterFunc)
    {
        _starters.Add(starterFunc);
    }

    [LuaMethod("addSystem")]
    public bool AddSystem(string name, LuaFunction updateFunc)
    {
        var table = _lua.NewTable();
        return _luaSystem.TryPutLuaSystem(name, updateFunc, table);
    }

    [LuaMethod("removeSystem")]
    public bool RemoveSystem(string name)
    {
        return _luaSystem.TryRemoveLuaSystem(name);
    }

    static void Postfix()
    {
        foreach (var starter in _starters)
        {
            starter.Call();
        }
    }
}