using HarmonyLib;
using System.Reflection;
using NLua;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries.Harmony;

public sealed class HarmonyLibrary : LuaLibrary
{
    [Dependency] private readonly NLua.Lua _lua = default!;
    [Dependency] private readonly HarmonyLib.Harmony _harmony = default!;

    public override string Name => "harmony";

    [LuaMethod("patch")]
    public void Patch(
        MethodBase original,
        LuaFunction? prefix = null,
        LuaFunction? postfix = null,
        LuaFunction? transpiler = null,
        LuaFunction? finalizer = null)
    {
        var processor = _harmony.CreateProcessor(original);
        if (prefix != null)
        {
            HarmonyLuaPool.Add(
                new LuaPoolData
                {
                    CFunction = original,
                    Function = prefix,
                    Type = HarmonyPatchType.Prefix,
                    State = _lua
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
                    Function = postfix,
                    Type = HarmonyPatchType.Postfix,
                    State = _lua
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
                    Function = transpiler,
                    Type = HarmonyPatchType.Transpiler,
                    State = _lua
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
                    Function = finalizer,
                    Type = HarmonyPatchType.Finalizer,
                    State = _lua
                }
            );
            HarmonyMethod method = new(GetType(), "LuaFinalizerProxy");
            processor.AddFinalizer(method);
        }

        processor.Patch();
    }

    [LuaMethod("unpatchAll")]
    public void UnpatchAll(string? id = null)
    {
        _harmony.UnpatchAll(id ?? _harmony.Id);
    }

    static bool LuaPrefixProxy(object __instance, object[] __args, MethodBase __originalMethod)
    {
        var (prefix, _) = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Prefix);

        var luaData = prefix.Call(__instance, __args);
        if (luaData.Length > 0 && luaData[0] as bool? == false)
            return false;

        return true;
    }
    static void LuaPostfixProxy(object __instance, object[] __args, ref object __result, MethodBase __originalMethod)
    {
        var (postfix, _) = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Postfix);

        postfix.Call(__instance, __args, __result);
    }
    static IEnumerable<CodeInstruction> LuaTranspilerProxy(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        var (transpiler, lua) = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Transpiler);

        var luaData = transpiler.Call(lua.EnumerableToTable(instructions));
        if (luaData.Length < 1 || luaData[0] is not LuaTable instructionsTable)
        {
            return instructions;
        }

        return lua.TableToEnumerable<CodeInstruction>(instructionsTable) ?? instructions;
    }
    static void LuaFinalizerProxy(Exception __exception, MethodBase __originalMethod)
    {
        var (finalizer, _) = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Finalizer);

        finalizer.Call(__exception);
    }
}