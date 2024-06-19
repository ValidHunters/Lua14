using Eluant;
using Eluant.ObjectBinding;
using Lua14.Lua.Objects;
using Robust.Shared.IoC;
using Robust.Shared.Reflection;
using System.Reflection;

namespace Lua14.Lua.Libraries;

public sealed class ReflectionLibrary(LuaRuntime lua) : Library(lua)
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

    [LuaMember("getType")]
    public LuaOpaqueClrObject? GetType(string path)
    {
        if (path.StartsWith("System") || path.StartsWith("Lua14"))
            return null;

        Type? type = _reflection.GetType(path);
        return new LuaOpaqueClrObject(type);
    }

    [LuaMember("getMethod")]
    public LuaOpaqueClrObject GetMethod(Type type, string methodName, Type[]? parameters = null)
    {
        MethodInfo? method;

        if (parameters is not null)
            method = type.GetMethod(methodName, _allFlags, parameters);
        else
            method = type.GetMethod(methodName, _allFlags);

        return new LuaOpaqueClrObject(method);
    }

    [LuaMember("getAllTypes")]
    public LuaValue FindAllTypes()
    {
        return _reflection
            .FindAllTypes()
            .Select(type => new LuaOpaqueClrObject(type))
            .ToLuaValue(Lua);
    }

    [LuaMember("findTypesWithAttribute")]
    public LuaValue FindTypesWithAttribute(Type attribute)
    {
        return _reflection
            .FindTypesWithAttribute(attribute)
            .Select(type => new LuaOpaqueClrObject(type))
            .ToLuaValue(Lua);
    }

    [LuaMember("getAllChildren")]
    public LuaValue GetAllChildren(Type baseType, bool inclusive = false)
    {
        return _reflection
            .GetAllChildren(baseType, inclusive)
            .Select(type => new LuaOpaqueClrObject(type))
            .ToLuaValue(Lua);
    }
}
