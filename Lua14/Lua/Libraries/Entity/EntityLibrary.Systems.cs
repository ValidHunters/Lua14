using Lua14.Systems;
using NLua;
using Robust.Shared.GameObjects;
using System.Diagnostics.CodeAnalysis;

namespace Lua14.Lua.Libraries.Entity;

public sealed partial class EntityLibrary
{
    [LuaMember(Name = "addSystem")]
    public void AddSystem(LuaTable table)
    {
        var systemTable = ToSystemTable(table);

        _entity.EntitySysManager.SystemLoaded += (sender, args) => OnSystemLoaded(args.System, systemTable);

        if (TryLuaSystem(out var luaSystem))
            luaSystem.PutLuaSystem(systemTable);
    }

    [LuaMember(Name = "removeSystem")]
    public void RemoveSystem(string id)
    {
        if (!TryLuaSystem(out var luaSystem))
            throw new Exception("Systems were not initialized yet.");

        luaSystem.RemoveLuaSystem(id);
    }

    private bool TryLuaSystem([NotNullWhen(true)] out LuaSystem? result)
    {
        result = null;

        try
        {
            result = _entity.SystemOrNull<LuaSystem>();
        }
        // if lua systems collection is null
        catch (NullReferenceException) { }

        return result != null;
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

    private static LuaFunction? GetLuaFunction(LuaTable table, string key)
    {
        if (table[key] != null && table[key] is not LuaFunction)
            throw new Exception($"Field \"{key}\" should be a function in your system table.");

        return table[key] as LuaFunction;
    }
}