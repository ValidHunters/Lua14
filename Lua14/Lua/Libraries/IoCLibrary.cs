using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries;

public sealed class IoCLibrary : LuaLibrary
{
    public override string Name => "IoC";

    [LuaMethod("resolve")]
    public object Resolve(Type type)
    {
        return IoCManager.ResolveType(type);
    }
}
