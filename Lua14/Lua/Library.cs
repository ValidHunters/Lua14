using NLua;

namespace Lua14.Lua;

public abstract class Library(NLua.Lua lua) : UserData(lua)
{
    protected virtual bool CreateGlobalTable { get { return true; } }
    protected abstract string Name { get; }

    [LuaHide]
    public virtual void Initialize() { }
    
    [LuaHide]
    public void Register() {
        if (!CreateGlobalTable)
            return;

        Lua[Name] = this;
    }
}
