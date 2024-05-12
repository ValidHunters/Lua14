using HarmonyLib;
using Lua14.Data;
using Lua14.Lua;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using System.Reflection;

namespace Lua14;

public sealed class EntryPoint : GameShared
{
    public override void PreInit()
    {
        Assembly subversionAssembly = Assembly.GetExecutingAssembly();
        SubverterPatch.Harm.PatchAll(subversionAssembly);
    }

    public override void PostInit()
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "IoC BuildGraph started.");
        IoCManager.BuildGraph();

        LoadLuaMods();
    }

    private static void LoadLuaMods()
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Getting lua mods folder..");
        var modFolder = LuaZipLoader.GetModsFolderPath();
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Start reading mod's zip files...");
        var mods = LuaZipLoader.ReadFolder(modFolder);

        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Start executing of all mods...");
        foreach (var mod in mods)
        {
            try
            {
                LuaRunner runner = new(mod);
                runner.ExecuteMain();
            }
            catch (Exception e)
            {
                MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Failed to start mod with name {mod.Config.Name}");
                MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Exception: {e}");
            }
        }
    }
}