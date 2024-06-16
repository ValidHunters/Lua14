using HarmonyLib;
using System.Reflection;
using Eluant;

namespace Lua14.Lua.Libraries.Harmony;

public struct LuaPoolData
{
    public LuaFunction Function;
    public MethodBase CFunction;
    public HarmonyPatchType Type;
    public LuaRuntime State;

    public readonly void Deconstruct(out LuaFunction func, out LuaRuntime lua)
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
