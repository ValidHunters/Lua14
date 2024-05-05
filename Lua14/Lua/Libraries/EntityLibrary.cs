using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries;

public sealed class EntityLibrary : LuaLibrary
{
    [Dependency] private readonly EntityManager _entity = default!;

    public override string Name => "entity";

    [LuaMethod("get")]
    public IComponent[] Get(EntityUid uid, params Type[] types)
    {
        List<IComponent> components = [];

        foreach (var type in types)
        {
            components.Add(_entity.GetComponent(uid, type));
        }

        return [.. components];
    }
}
