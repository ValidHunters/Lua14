using HarmonyLib;
using LuaState = NLua.Native.LuaState;
using NLua;
using System.Reflection;

namespace Lua14.Lua.Patches;

/// <summary>
/// extracts arrays as tuples
/// </summary>
[HarmonyPatch(typeof(ObjectTranslator), nameof(ObjectTranslator.PushMultiple))]
public static class PushMultiplePatch
{
    private static readonly MethodInfo m_Push = typeof(ObjectTranslator).GetMethod("Push", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [HarmonyPrefix]
    static bool Prefix(ref int __result, ObjectTranslator __instance, LuaState luaState, object o)
    {
        if (o is object[] tuple)
        {
            for (int i = 0; i < tuple.Length; ++i)
            {
                m_Push.Invoke(
                    __instance,
                    [luaState, tuple[i]]
                );
            }

            __result = tuple.Length;
            return false;
        }

        return true;
    }
}
