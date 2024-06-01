using Lua14.Lua.Systems;
using NLua;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries;

public class LuaSystemLibrary(NLua.Lua lua) : Library(lua)
{
    [Dependency] private readonly EntityManager _entity = default!;

    protected override string Name => "lua_sys";

    [LuaMember(Name = "addSystem")]
    public void AddSystem(LuaTable table)
    {
        _entity.EntitySysManager.SystemLoaded += (sender, args) => OnSystemLoaded(args.System, ToSystemTable(table));

        if (_entity.Initialized && _entity.TrySystem<LuaSystem>(out var luaSystem))
            luaSystem.PutLuaSystem(ToSystemTable(table));
    }

    [LuaMember(Name = "removeSystem")]
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
        if (table["Id"] is not string id)
            throw new Exception("Field \"Id\" should be a string in your system table.");

    	return new LuaSystemTable
    	{
        	Id = id,
        	Initialize = GetLuaFunction(table, "Initialize"),
        	Update = GetLuaFunction(table, "Update"),
        	Shutdown = GetLuaFunction(table, "Shutdown")
        };
	}

	private static LuaFunction GetLuaFunction(LuaTable table, string key)
	{
    	if (table[key] != null && table[key] is not LuaFunction)
        	throw new Exception($"Field \"{key}\" should be a function in your system table.");

	    return table[key] as LuaFunction;
	}
}