using NLua;
using System.Reflection;

namespace Lua14.Lua;

public class LuaUserdata
{
    [LuaHide]
    public readonly LuaUserData Userdata;
    protected readonly NLua.Lua Lua;

    public LuaUserdata(NLua.Lua state)
    {
        Lua = state;
        Userdata = Lua.ObjectToUserdata(this);

        RegisterMetaMethods();
    }

    private void RegisterMetaMethods()
    {
        LuaTable metatable = Lua.GetMetatable(Userdata) ?? throw new Exception("No metatable found for a c# userdata.");

        RegisterFunction(metatable, "__iter", "Iter");
        RegisterFunction(metatable, "__call", "Call");
    }

    private void RegisterFunction(LuaTable table, string index, string methodName)
    {
        Lua.SetFunction(
            table,
            index,
            GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!,
            this
        );
    }

    /// <summary>
    /// Fires when the table is called like a function.
    /// </summary>
    /// <param name="args">The arguments that were passed.</param>
    /// <exception cref="NotImplementedException"></exception>
    protected virtual LuaFunction Call(LuaTable self, params object[] args)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 	Used to denote a custom iterator when using generalized iteration.
    /// </summary>
    protected virtual LuaFunction Iter(LuaTable self)
    {
        throw new NotImplementedException();
    }
}
