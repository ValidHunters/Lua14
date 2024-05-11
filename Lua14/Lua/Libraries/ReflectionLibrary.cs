using NLua;
using Robust.Shared.IoC;
using Robust.Shared.Reflection;
using System.Reflection;

namespace Lua14.Lua.Libraries;

public sealed class ReflectionLibrary(NLua.Lua lua) : LuaLibrary(lua)
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

    protected override string Name => "reflection";

    [LuaMember(Name = "getType")]
    public Type? GetType(string path)
    {
        if (path.StartsWith("System") || path.StartsWith("Lua14"))
            return null;

        return _reflection.GetType(path);
    }

    [LuaMember(Name = "getMethod")]
    public MethodInfo? GetMethod(Type type, string methodName, Type[]? parameters = null)
    {
        if (parameters is not null)
            return type.GetMethod(methodName, _allFlags, parameters);

        return type.GetMethod(methodName, _allFlags);
    }

    [LuaMember(Name = "getAllTypes")]
    public LuaTable FindAllTypes()
    {
        IEnumerable<Type> types = _reflection.FindAllTypes();

        return Lua.EnumerableToTable(types);
    }

    [LuaMember(Name = "findTypesWithAttribute")]
    public LuaTable FindTypesWithAttribute(Type attribute)
    {
        IEnumerable<Type> types = _reflection.FindTypesWithAttribute(attribute);

        return Lua.EnumerableToTable(types);
    }

    [LuaMember(Name = "getAllChildren")]
    public LuaTable GetAllChildren(Type baseType, bool inclusive = false)
    {
        IEnumerable<Type> types = _reflection.GetAllChildren(baseType, inclusive);

        return Lua.EnumerableToTable(types);
    }
}
