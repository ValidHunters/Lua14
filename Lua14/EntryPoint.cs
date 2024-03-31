using System.Reflection;
using Lua14.Data;
using Lua14.Lua;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;

namespace Lua14;

public sealed class EntryPoint : GameShared
{
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
        foreach (var runner in mods.Select(mod => new LuaRunner(mod)))
        {
            try
            {
                runner.ExecuteMain();
            }
            catch (Exception e)
            {
                MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Failed to start mod with name {runner.Mod.Config.Name}");
                MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Exception: {e}");
            }
        }
    }
}