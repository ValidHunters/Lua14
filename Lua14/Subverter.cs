using HarmonyLib;

public static class SubverterPatch
{
    public static string Name = "Lua14";
    public static string Description = "Make epic ss14 mods with lua";
    public static Harmony Harm = new("com.lua14.patch");
}