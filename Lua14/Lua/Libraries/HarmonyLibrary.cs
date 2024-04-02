using Lua14.Data;
using HarmonyLib;
using NLua;
using System.Reflection;

namespace Lua14.Lua.Libraries;

public sealed class HarmonyLibrary(NLua.Lua lua, LuaMod mod, LuaLogger log) : LuaLibrary(lua, mod, log)
{
    public override string Name => "harmony";

    [LuaMethod("patch")]
    public void Patch(
        MethodBase original,
        LuaFunction? prefix = null,
        LuaFunction? postfix = null,
        LuaFunction? transpiler = null,
        LuaFunction? finalizer = null)
    {
        try
        {
            var processor = SubverterPatch.Harm.CreateProcessor(original);
            if (prefix != null)
            {
                HarmonyLuaPool.Add(
                    new LuaPoolData(
                        original,
                        prefix,
                        HarmonyPatchType.Prefix
                    )
                );
                HarmonyMethod method = new(GetType(), "LuaPrefixProxy");
                processor.AddPrefix(method);
            }
            if (postfix != null)
            {
                HarmonyLuaPool.Add(
                    new LuaPoolData(
                        original,
                        postfix,
                        HarmonyPatchType.Postfix
                    )
                );
                HarmonyMethod method = new(GetType(), "LuaPostfixProxy");
                processor.AddPostfix(method);
            }
            if (transpiler != null)
            {
                HarmonyLuaPool.Add(
                    new LuaPoolData(
                        original,
                        transpiler,
                        HarmonyPatchType.Postfix
                    )
                );
                HarmonyMethod method = new(GetType(), "LuaTranspilerProxy");
                processor.AddTranspiler(method);
            }
            if (finalizer != null)
            {
                HarmonyLuaPool.Add(
                    new LuaPoolData(
                        original,
                        finalizer,
                        HarmonyPatchType.Finalizer
                    )
                );
                HarmonyMethod method = new(GetType(), "LuaFinalizerProxy");
                processor.AddFinalizer(method);
            }

            processor.Patch();
        } catch(Exception e)
        {
            Logger.Info(e.ToString());
        }
    }

    bool LuaPrefixProxy(object __instance, object[] __args, MethodBase __originalMethod)
    {
        LuaPoolData data = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Prefix);
        LuaFunction prefix = data.Function;

        var luaData = prefix.Call(__instance, __args);
        if (luaData.Length > 0 && luaData[0] as bool? == false)
            return false;

        return true;
    }
    void LuaPostfixProxy(object __instance, object[] __args, ref object __result, MethodBase __originalMethod)
    {
        LuaPoolData data = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Postfix);
        LuaFunction postfix = data.Function;

        postfix.Call(__instance, __args, __result);
    }
    IEnumerable<CodeInstruction> LuaTranspilerProxy(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        LuaPoolData data = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Transpiler);
        LuaFunction transpiler = data.Function;

        var luaData = transpiler.Call(instructions);
        if (luaData.Length < 1 || luaData[0] is not LuaTable instructionsTable)
        {
            Logger.Error("Lua transpiler function didnt return a table.");
            return instructions;
        }

        return TableToEnumerable<CodeInstruction>(instructionsTable) ?? instructions;
    }
    void LuaFinalizerProxy(Exception __exception, MethodBase __originalMethod)
    {
        LuaPoolData data = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Finalizer);
        LuaFunction finalizer = data.Function;

        finalizer.Call(__exception);
    }

    private struct LuaPoolData(MethodBase cFunction, LuaFunction func, HarmonyPatchType type)
    {
        public LuaFunction Function { get; } = func;
        public MethodBase CFunction { get; } = cFunction;
        public HarmonyPatchType Type { get; } = type;
    }
    static private class HarmonyLuaPool
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
}
