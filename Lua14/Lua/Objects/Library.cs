using Eluant;

namespace Lua14.Lua.Objects;

public abstract class Library
{
    protected readonly LuaRuntime Lua;

    protected virtual bool CreateGlobalTable => true;
    protected abstract string Name { get; }

    protected Library(LuaRuntime lua)
    {
        Lua = lua;
    }

    public virtual void Initialize() { }

    public void Register()
    {
        if (!CreateGlobalTable)
            return;

        Lua.Globals[Name] = new LuaTransparentClrObject(this);
    }
}
