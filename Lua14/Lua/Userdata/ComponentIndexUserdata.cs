using NLua;
using Robust.Shared.GameObjects;

namespace Lua14.Lua.Userdata;

public sealed class ComponentIndexUserdata : LuaUserdata
{
    [LuaMember(Name = "type")]
    public Type Type;

    public ComponentIndexUserdata(NLua.Lua lua, Type type) : base(lua)
    {
        if (!typeof(IComponent).IsAssignableFrom(type))
            throw new Exception("Cant create a ComponentIndex because the specified type is not a component.");

        Type = type;
    }

    protected override object? Call(LuaUserdata self, params object[] args)
    {
        if (args.Length < 1)
            return Activator.CreateInstance(Type);

        if (args[0] is not LuaTable parameters)
            throw new Exception("First argument should be a table.");

        var paramsDict = Lua.TableToDictionary<string, object?>(parameters) ?? throw new Exception("First argument should be a table<string, any>.");
        var comp = Activator.CreateInstance(Type)!;

        LuaUserData compUserdata = Lua.ObjectToUserdata(comp);
        foreach (var (key, value) in paramsDict)
        {
            compUserdata[key] = value;
        }

        return comp;
    }
}
