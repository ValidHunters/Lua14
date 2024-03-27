using System.Reflection;

namespace Lua14.Lua;

public abstract class LuaLibrary
{
    protected LuaLibrary(NLua.Lua lua, LuaLogger log)
    {
        Lua = lua;
        Logger = log;
    }

    protected NLua.Lua Lua { get; }
    protected LuaLogger Logger { get; }
    public abstract string Name { get; }

    public void Register() {
        Lua.NewTable(Name);

        Type luaAttr = typeof(LuaMethodAttribute);

        var methods = GetType().GetMethods().Where(it => it.GetCustomAttributes(luaAttr, false).Length != 0);
        foreach(var method in methods)
        {
            var attr = (LuaMethodAttribute)method.GetCustomAttribute(luaAttr)!;
            Lua.RegisterFunction(Name + "." + attr.Name, this, method);
        }
    }
}
