using HarmonyLib;
using NLua;
using System.Reflection;
using System.Reflection.Emit;

namespace Lua14.Patches;

/// <summary>
/// makes instance method callable without a target object
/// example: fs.readfile("/e")
/// instead of fs.readfile(fs, "/e")
/// </summary>
[HarmonyPatch(typeof(MetaFunctions), "GetMember")]
public static class GetMemberPatch
{
    private static readonly FieldInfo F_Translator = AccessTools.Field(typeof(MetaFunctions), "_translator")
        ?? throw new Exception("Field _translator doesnt exist on NLua.MetaFunctions");

    private static readonly Type LuaMethodWrapperType = AccessTools.TypeByName("NLua.Method.LuaMethodWrapper");
    private static readonly ConstructorInfo TargetWrapperCtor = AccessTools.Constructor(LuaMethodWrapperType, [typeof(ObjectTranslator), typeof(object), typeof(ProxyType), typeof(MethodBase)])
        ?? throw new Exception("Target constructor was not found");

    /// <summary>
    /// replaces "var methodWrapper = new LuaMethodWrapper(_translator, objType, methodName, bindingType);"
    /// with "var methodWrapper = new LuaMethodWrapper(_translator, obj, objType, (MethodBase) member);"
    /// 
    /// https://github.com/NLua/NLua/blob/4c5705b826652789db096e983d86ee514fe5c8ff/src/Metatables.cs#L924C25-L924C113
    /// </summary>
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        
        var translatorFields = codes.Where(instr => instr.LoadsField(F_Translator)).ToList();
        foreach (var instruction in translatorFields)
        {
            var index = codes.IndexOf(instruction);
            if (codes[index + 1].opcode != OpCodes.Ldarg_2 || // loads ProxyType
                codes[index + 2].opcode != OpCodes.Ldarg_S || // loads string
                codes[index + 3].opcode != OpCodes.Ldarg_S || // loads BindingFlags
                codes[index + 4].opcode != OpCodes.Newobj)
            {
                continue;
            }

            codes[index + 1] = new CodeInstruction(OpCodes.Ldarg_S, 3); // loads object
            codes[index + 2] = new CodeInstruction(OpCodes.Ldarg_2); // loads ProxyType
            codes[index + 3] = new CodeInstruction(OpCodes.Ldloc_1); // loads MemberInfo
            codes[index + 4] = new CodeInstruction(OpCodes.Newobj, TargetWrapperCtor);

            codes.Insert(index + 4, new CodeInstruction(OpCodes.Castclass, typeof(MethodBase))); // pops MemberInfo and loads MethodBase
        }

        return codes.AsEnumerable();
    }
}
