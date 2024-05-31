using HarmonyLib;
using System.Reflection;
using NLua;
using Robust.Shared.IoC;
using Lua14.Lua.Data.Structures;

namespace Lua14.Lua.Libraries.Harmony;

public sealed class HarmonyLibrary(NLua.Lua lua) : LuaLibrary(lua)
{
    [Dependency] private readonly Mod _mod = default!;
    private HarmonyLib.Harmony _harmony = default!;

    public override void Initialize()
    {
        _harmony = new HarmonyLib.Harmony("lua." + _mod.Config.Name);
    }

    protected override string Name => "harmony";

    [LuaMember(Name = "patch")]
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
                    Function = prefix,
                    Type = HarmonyPatchType.Prefix,
                    State = Lua
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
                    State = Lua
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
                    State = Lua
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
                    State = Lua
                }
            );
            HarmonyMethod method = new(GetType(), "LuaFinalizerProxy");
            processor.AddFinalizer(method);
        }

        processor.Patch();
    }

    [LuaMember(Name = "unpatch")]
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

    [LuaMember(Name = "unpatchAll")]
    public void UnpatchAll(string id = null)
    {
        _harmony.UnpatchAll(id ?? _harmony.Id);
    }

    private static bool LuaPrefixProxy(object __instance, object[] __args, MethodBase __originalMethod)
    {
        var (prefix, lua) = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Prefix);

        var luaData = prefix.Call(__instance, lua.EnumerableToTable(__args));
        if (luaData.Length > 0 && luaData[0] as bool? == false)
            return false;

        return true;
    }
    private static void LuaPostfixProxy(object __instance, object[] __args, ref object __result, MethodBase __originalMethod)
    {
        var (postfix, lua) = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Postfix);

        postfix.Call(__instance, lua.EnumerableToTable(__args), __result);
    }
    private static IEnumerable<CodeInstruction> LuaTranspilerProxy(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        var (transpiler, lua) = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Transpiler);

        var luaData = transpiler.Call(lua.EnumerableToTable(instructions));
        if (luaData.Length < 1 || luaData[0] is not LuaTable instructionsTable)
        {
            return instructions;
        }

        return lua.TableToEnumerable<CodeInstruction>(instructionsTable) ?? instructions;
    }
    private static void LuaFinalizerProxy(Exception __exception, MethodBase __originalMethod)
    {
        var (finalizer, _) = HarmonyLuaPool.Get(__originalMethod, HarmonyPatchType.Finalizer);

        finalizer.Call(__exception);
    }
}