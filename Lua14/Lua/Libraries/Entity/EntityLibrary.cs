using Lua14.Lua.Userdata;
using NLua;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries.Entity;

public sealed partial class EntityLibrary(NLua.Lua lua) : LuaLibrary(lua)
{
    [Dependency] private readonly EntityManager _entity = default!;

    protected override string Name => "entity";

    [LuaMember(Name = "index")]
    public ComponentIndexUserdata[] Index(params Type[] types)
    {
        return types
            .Select(compType => new ComponentIndexUserdata(Lua, compType))
            .ToArray();
    }

    [LuaMember(Name = "get")]
    public IComponent[] Get(EntityUid uid, params ComponentIndexUserdata[] indexes)
    {
        return indexes
            .Select(compType => _entity.GetComponent(uid, compType.Type))
            .ToArray();
    }

    [LuaMember(Name = "query")]
    public EntityQueryUserdata Query(params ComponentIndexUserdata[] indexes)
    {
        return new EntityQueryUserdata(Lua, _entity, indexes);
    }

    [LuaMember(Name = "insert")]
    public void Insert(EntityUid uid, params IComponent[] comps)
    {
        foreach (var comp in comps)
        {
            _entity.AddComponent(uid, comp, true);
        }
    }

    [LuaMember(Name = "remove")]
    public void Remove(EntityUid uid, params ComponentIndexUserdata[] indexes)
    {
        foreach (var index in indexes)
        {
            _entity.RemoveComponent(uid, index.Type);
        }
    }

    [LuaMember(Name = "despawn")]
    public void Despawn(EntityUid uid)
    {
        _entity.QueueDeleteEntity(uid);
    }

    [LuaMember(Name = "size")]
    public int Size()
    {
        return _entity.EntityCount;
    }
}
