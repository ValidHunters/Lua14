using Lua14.Systems;
using NLua;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;

namespace Lua14.Lua.Libraries;

public class LuaSystemLibrary : LuaLibrary
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    private LuaSystem _luaSystem = null!;
    [Dependency] private readonly NLua.Lua _lua = default!;
    [Dependency] private readonly LuaLogger _logger = default!;

    public override void Initialize()
    {
        if (!_entityManager.Initialized)
            _logger.Warning("EntManager not inited");
        var sys = _entityManager.System<SharedAppearanceSystem>();
    }

    public override string Name => "lua-sys";

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
}