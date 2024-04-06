using System.Reflection;

namespace Lua14.Lua;

public abstract partial class LuaLibrary
{
    public virtual bool IsLibraryGlobal { get { return false; } }
    public abstract string Name { get; }

    public virtual void Initialize() { }
    public void Register(NLua.Lua lua) {
        if (!IsLibraryGlobal)
            lua.NewTable(Name);

        var methods = GetType().GetMethods().Where(it => it.GetCustomAttributes(typeof(LuaMethodAttribute), false).Length != 0);
        foreach(var method in methods)
        {
            var attr = method.GetCustomAttribute<LuaMethodAttribute>()!;
            if (IsLibraryGlobal)
                lua.RegisterFunction(attr.Name, this, method);
            else
                lua.RegisterFunction(Name + "." + attr.Name, this, method);
        }
    }
}
