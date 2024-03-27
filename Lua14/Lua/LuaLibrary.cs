using Lua14.Data;
using System.Reflection;

namespace Lua14.Lua;

public abstract class LuaLibrary
{
    protected LuaLibrary(NLua.Lua lua, LuaMod mod, LuaLogger log)
    {
        Lua = lua;
        Mod = mod;
        Logger = log;
    }

    protected NLua.Lua Lua { get; }
    protected LuaMod Mod { get; }
    protected LuaLogger Logger { get; }
    public virtual bool IsLibraryGlobal { get { return false; } }
    public abstract string Name { get; }

    public void Register() {
        if (!IsLibraryGlobal)
            Lua.NewTable(Name);

        var methods = GetType().GetMethods().Where(it => it.GetCustomAttributes(typeof(LuaMethodAttribute), false).Length != 0);
        foreach(var method in methods)
        {
            var attr = method.GetCustomAttribute<LuaMethodAttribute>()!;
            if (IsLibraryGlobal)
                Lua.RegisterFunction(Name + "." + attr.Name, this, method);
            else
                Lua.RegisterFunction(attr.Name, this, method);
        }
    }
}
