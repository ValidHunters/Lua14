using Lua14.Systems;
using NLua;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries;

public class LuaSystemLibrary : LuaLibrary
{
    [Dependency] private readonly EntityManager _entity = default!;

    public override string Name => "lua_sys";

    [LuaMethod("addSystem")]
    public void AddSystem(LuaTable table)
    {
        _entity.EntitySysManager.SystemLoaded += (sender, args) => OnSystemLoaded(args.System, ToSystemTable(table));

        if (_entity.Initialized && _entity.TrySystem<LuaSystem>(out var luaSystem))
            luaSystem.PutLuaSystem(ToSystemTable(table));
    }

    [LuaMethod("removeSystem")]
    public void RemoveSystem(string id)
    {
        if (!_entity.Initialized || !_entity.TrySystem<LuaSystem>(out var luaSystem))
            throw new Exception("Systems were not Initialized yet.");

        luaSystem.RemoveLuaSystem(id);
    }

    private static void OnSystemLoaded(IEntitySystem system, LuaSystemTable table)
    {
        if (system is not LuaSystem luaSystem)
            return;

        luaSystem.PutLuaSystem(table);
    }

	private static LuaSystemTable ToSystemTable(LuaTable table)
	{
	    string id = table["Id"] as string;
	    if (id == null)
        	throw new Exception("Field "Id" should be a string in your system table.");

	    LuaFunction initialize = GetLuaFunction(table, "Initialize");
	    LuaFunction update = GetLuaFunction(table, "Update");
    	LuaFunction shutdown = GetLuaFunction(table, "Shutdown");

    	return new LuaSystemTable
    	{
        	Id = id,
        	Initialize = initialize,
        	Update = update,
        	Shutdown = shutdown
    	};
	}

	private static LuaFunction GetLuaFunction(LuaTable table, string key)
	{
    	if (table[key] != null && table[key] is not LuaFunction)
        	throw new Exception($"Field "{key}" should be a function in your system table.");

	    return table[key] as LuaFunction;
	}
}