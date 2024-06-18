using HarmonyLib;
using System.Reflection;
using Eluant;

namespace Lua14.Lua.Libraries.Harmony;

public struct LuaPoolData
{
    public LuaFunction Function;
    public MethodBase CFunction;
    public HarmonyPatchType Type;

    public readonly void Deconstruct(out LuaFunction func)
    {
        func = Function;
    }
}

public static class HarmonyLuaPool
{
    private static readonly HashSet<LuaPoolData> _poolData = [];

    public static LuaPoolData Get(MethodBase method, HarmonyPatchType type, out LuaFunction function)
    {
        var ret = _poolData.Where(it => it.Type == type && it.CFunction == method).Single();

        function = ret.Function;
        return ret;
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
