using HarmonyLib;
using System.Reflection;

namespace Lua14.Lua.Libraries.Harmony;

public struct LuaPoolData
{
    public NLua.LuaFunction Function;
    public MethodBase CFunction;
    public HarmonyPatchType Type;
    public NLua.Lua State;

    public readonly void Deconstruct(out NLua.LuaFunction func, out NLua.Lua lua)
    {
        func = Function;
        lua = State;
    }
}

public static class HarmonyLuaPool
{
    private static readonly HashSet<LuaPoolData> _poolData = [];

    public static LuaPoolData Get(MethodBase method, HarmonyPatchType type)
    {
        return _poolData.Where(it => it.Type == type && it.CFunction == method).Single();
    }
    public static void Add(LuaPoolData data)
    {
        _poolData.Add(data);
    }
    public static void Remove(LuaPoolData data)
    {
        _poolData.Remove(data);
    }
}
