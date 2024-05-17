using Lua14.Lua.Userdata;
using NLua;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries.Entity;

public sealed partial class EntityLibrary(NLua.Lua lua) : LuaLibrary(lua)
{
    [Dependency] private readonly EntityManager _entity = default!;

    protected override string Name => "entity";

    [LuaMember(Name = "get")]
    public IComponent[] Get(EntityUid uid, params Type[] types)
    {
        return types
            .Select(compType => _entity.GetComponent(uid, compType))
            .ToArray();
    }

    [LuaMember(Name = "query")]
    public LuaUserData Query(params Type[] types)
    {
        return new EntityQueryUserdata(Lua, _entity, types).Userdata;
    }
}
