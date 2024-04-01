using Lua14.Data;
using HarmonyLib;
using NLua;
using Robust.Shared.IoC;
using System.Reflection;

namespace Lua14.Lua.Libraries;

public sealed class HarmonyLibrary(NLua.Lua lua, LuaMod mod, LuaLogger log) : LuaLibrary(lua, mod, log)
{
    [Dependency] private readonly ExtensionLibrary _extensions = default!;

    public override string Name => "harmony";

    [LuaMethod("patch")]
    public void Patch(
        MethodBase original,
        LuaFunction? prefix = null,
        LuaFunction? postfix = null,
        LuaFunction? transpiler = null,
        LuaFunction? finalizer = null)
    {
        var processor = SubverterPatch.Harm.CreateProcessor(original);
        if (prefix != null)
        {
            Delegate prefixDelegate = (object __instance, object[] __args) =>
            {
                var prefixTable = _extensions.NewTable();
                prefixTable["instance"] = __instance;
                prefixTable["args"] = __args;

                object[] data = prefix.Call(prefixTable);
                if (data.Length > 0 && data[0] == (object)false)
                    return false;

                return true;
            };
            HarmonyMethod prefixMethod = new(prefixDelegate);
            processor.AddPrefix(prefixMethod);
        }
        if (postfix != null)
        {
            Delegate postfixDelegate = (object __instance, object[] __args, ref object __result) =>
            {
                var postfixTable = _extensions.NewTable();
                postfixTable["instance"] = __instance;
                postfixTable["args"] = __args;
                postfixTable["result"] = __result;

                postfix.Call(postfixTable);
            };
            HarmonyMethod postfixMethod = new(postfixDelegate);
            processor.AddPostfix(postfixMethod);
        }
        if (transpiler != null)
        {
            Delegate transpilerDelegate = (IEnumerable<CodeInstruction> instructions) =>
            {
                object[] data = transpiler.Call(instructions);
                if (data.Length > 0 && data[0] != null)
                    return data[0];

                return instructions;
            };
            HarmonyMethod transpilerMethod = new(transpilerDelegate);
            processor.AddTranspiler(transpilerMethod);
        }
        if (finalizer != null)
        {
            Delegate finalizerDelegate = (Exception __exception) =>
            {
                finalizer.Call(__exception);
            };
            HarmonyMethod finalizerMethod = new(finalizerDelegate);
            processor.AddFinalizer(finalizerMethod);
        }

        processor.Patch();
    }
}
