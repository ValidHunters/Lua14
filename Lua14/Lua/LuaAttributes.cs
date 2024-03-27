namespace Lua14.Lua;

[AttributeUsage(AttributeTargets.Method)]
public class LuaMethodAttribute: Attribute
{
    public LuaMethodAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
