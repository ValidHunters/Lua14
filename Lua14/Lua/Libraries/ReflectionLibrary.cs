using Lua14.Data;
using NLua;
using Robust.Shared.IoC;
using Robust.Shared.Reflection;
using System.Reflection;

namespace Lua14.Lua.Libraries;

public sealed class ReflectionLibrary(NLua.Lua lua, LuaMod mod, LuaLogger log) : LuaLibrary(lua, mod, log)
{
    [Dependency] private readonly IReflectionManager _reflection = default!;

    private static readonly BindingFlags _allFlags = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.GetField
            | BindingFlags.SetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty;

    public override string Name => "reflection";

    [LuaMethod("getType")]
    public Type? GetType(string path)
    {
        if (path.StartsWith("System") || path.StartsWith("Lua14"))
            return null;

        return _reflection.GetType(path);
    }
    [LuaMethod("getMethod")]
    public MethodInfo? GetMethod(Type type, string methodName, Type[]? parameters = null)
    {
        if (parameters is not null)
            return type.GetMethod(methodName, _allFlags, parameters);

        return type.GetMethod(methodName, _allFlags);
    }
    [LuaMethod("getAllTypes")]
    public LuaTable FindAllTypes()
    {
        IEnumerable<Type> types = _reflection.FindAllTypes();

        return EnumerableToTable(types);
    }
    [LuaMethod("findTypesWithAttribute")]
    public LuaTable FindTypesWithAttribute(Type attribute)
    {
        IEnumerable<Type> types = _reflection.FindTypesWithAttribute(attribute);

        return EnumerableToTable(types);
    }
    [LuaMethod("getAllChildren")]
    public LuaTable GetAllChildren(Type baseType, bool inclusive = false)
    {
        IEnumerable<Type> types = _reflection.GetAllChildren(baseType, inclusive);

        return EnumerableToTable(types);
    }
}
