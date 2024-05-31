using HarmonyLib;
using NLua;
using System.Reflection;

namespace Lua14.Lua;

public class LuaUserdata
{
    private static readonly MethodInfo M_Call = AccessTools.Method(typeof(LuaFunction), "Call");

    protected readonly NLua.Lua Lua;
    protected readonly LuaTable Metatable;

    // metamethods
    private readonly LuaFunction __index;
    private readonly LuaFunction __newindex;
    private readonly LuaFunction __call;

    public LuaUserdata(NLua.Lua state)
    {
        Lua = state;
        Metatable = Lua.GetMetatable(this) ?? throw new Exception("No metatable found for a c# userdata.");

        __index = GetMetaMethod("__index");
        __newindex = GetMetaMethod("__newindex");
        __call = GetMetaMethod("__call");

        RegisterMetaMethods();
    }

    private LuaFunction GetMetaMethod(string metamethod)
    {
        return (LuaFunction)Metatable[metamethod];
    }

    private void RegisterMetaMethods()
    {
        RegisterFunction(Metatable, "__index", "Index");
        RegisterFunction(Metatable, "__newindex", "NewIndex");
        RegisterFunction(Metatable, "__iter", "Iter");
        RegisterFunction(Metatable, "__call", "Call");
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
    /// Fires when table[index] is indexed.
    /// </summary>
    protected virtual object Index(LuaUserdata self, object index)
    {
        return __index.Call(self, index);
    }

    /// <summary>
    /// Fires when table[index] tries to be set (table[index] = value).
    /// </summary>
    protected virtual void NewIndex(LuaUserdata self, object index, object value)
    {
        __newindex.Call(self, index, value);
    }

    /// <summary>
    /// Fires when the table is called like a function.
    /// By default its the current object constructor.
    /// </summary>
    /// <param name="args">The arguments that were passed.</param>
    protected virtual object Call(LuaUserdata self, params object[] args)
    {
        return M_Call.Invoke(__call, [self, ..args]);
    }

    /// <summary>
    /// 	Used to denote a custom iterator when using generalized iteration.
    /// </summary>
    protected virtual LuaFunction Iter(LuaUserdata self)
    {
        throw new NotImplementedException();
    }

    public object this[string index]
    {
        get => __index.Call(this, index);
        set => __newindex.Call(this, index, value);
    }
}
