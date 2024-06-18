using HarmonyLib;
using System.Reflection;
using Robust.Shared.IoC;
using Lua14.Lua.Data.Structures;
using Lua14.Lua.Objects;
using Eluant;
using Eluant.ObjectBinding;

namespace Lua14.Lua.Libraries.Harmony;

public sealed class HarmonyLibrary(LuaRuntime lua) : Library(lua)
{
    [Dependency] private readonly Mod _mod = default!;
    private HarmonyLib.Harmony _harmony = default!;

    public override void Initialize()
    {
        _harmony = new HarmonyLib.Harmony("lua." + _mod.Name);
    }

    protected override string Name => "harmony";

    [LuaMember("patch")]
    public void Patch(
        MethodBase original,
        LuaFunction prefix = null,
        LuaFunction postfix = null,
        LuaFunction transpiler = null,
        LuaFunction finalizer = null)
    {
        var processor = _harmony.CreateProcessor(original);
        if (prefix != null)
        {
            HarmonyLuaPool.Add(
                new LuaPoolData
                {
                    CFunction = original,
                    Function = (LuaFunction)prefix.CopyReference(),
                    Type = HarmonyPatchType.Prefix
                }
            );
            HarmonyMethod method = new(GetType(), "LuaPrefixProxy");
            processor.AddPrefix(method);
        }
        if (postfix != null)
        {
            HarmonyLuaPool.Add(
                new LuaPoolData
                {
                    CFunction = original,
                    Function = (LuaFunction)postfix.CopyReference(),
                    Type = HarmonyPatchType.Postfix
                }
            );
            HarmonyMethod method = new(GetType(), "LuaPostfixProxy");
            processor.AddPostfix(method);
        }
        if (transpiler != null)
        {
            HarmonyLuaPool.Add(
                new LuaPoolData
                {
                    CFunction = original,
                    Function = (LuaFunction)transpiler.CopyReference(),
                    Type = HarmonyPatchType.Transpiler
                }
            );
            HarmonyMethod method = new(GetType(), "LuaTranspilerProxy");
            processor.AddTranspiler(method);
        }
        if (finalizer != null)
        {
            HarmonyLuaPool.Add(
                new LuaPoolData
                {
                    CFunction = original,
                    Function = (LuaFunction)finalizer.CopyReference(),
                    Type = HarmonyPatchType.Finalizer
                }
            );
            HarmonyMethod method = new(GetType(), "LuaFinalizerProxy");
            processor.AddFinalizer(method);
        }

        processor.Patch();
    }

    [LuaMember("unpatch")]
    public void Unpatch(MethodBase original, string type = "all", string id = null)
    {
        switch (type)
        {
            case "prefix":
                _harmony.Unpatch(original, HarmonyPatchType.Prefix, id ?? _harmony.Id);
                break;
            case "postfix":
                _harmony.Unpatch(original, HarmonyPatchType.Postfix, id ?? _harmony.Id);
                break;
            case "transpiler":
                _harmony.Unpatch(original, HarmonyPatchType.Transpiler, id ?? _harmony.Id);
                break;
            case "finalizer":
                _harmony.Unpatch(original, HarmonyPatchType.Finalizer, id ?? _harmony.Id);
                break;
            case "all":
                _harmony.Unpatch(original, HarmonyPatchType.All, id ?? _harmony.Id);
                break;
        }
    }

    [LuaMember("unpatchAll")]
    public void UnpatchAll(string id = null)
    {
        _harmony.UnpatchAll(id ?? _harmony.Id);
    }

    private static bool LuaPrefixProxy(object __instance, object[] __args, MethodBase __originalMethod)
    {
        HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Prefix, out var prefix);

        using LuaVararg functionResult = prefix.Call(__instance, __args);
        if (functionResult.Count > 0)
        {
            using LuaBoolean prefixResult = functionResult[0] as LuaBoolean;
            if (prefixResult == false)
                return false;
        }

        return true;
    }

    private static void LuaPostfixProxy(object __instance, object[] __args, ref object __result, MethodBase __originalMethod)
    {
        HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Postfix, out var postfix);

        postfix.Call(__instance, __args, __result);
    }

    private static IEnumerable<CodeInstruction> LuaTranspilerProxy(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Transpiler, out var transpiler);

        using LuaVararg functionResult = transpiler.Call(instructions);
        if (functionResult.Count > 0)
        {
            using LuaValue transpilerResult = functionResult[0];

            if (transpilerResult.TryGetClrValue<CodeInstruction[]>(out var luaInstructions))
                return luaInstructions;
        }

        return instructions;
    }

    private static void LuaFinalizerProxy(Exception __exception, MethodBase __originalMethod)
    {
        HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Finalizer, out var finalizer);

        finalizer.Call(__exception);
    }
}