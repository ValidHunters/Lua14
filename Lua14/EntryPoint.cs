using Lua14.Data;
using Lua14.Lua;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;

namespace Lua14;

public sealed class EntryPoint : GameShared
{
    public override void PostInit()
    {
        IoCManager.BuildGraph();

        LoadLuaMods();
    }

    private void LoadLuaMods()
    {
        string modFolder = LuaZipLoader.GetModsFolderPath();
        List<LuaMod> mods = LuaZipLoader.ReadFolder(modFolder);

        foreach (LuaMod mod in mods)
        {
            LuaRunner runner = new(mod);
            runner.ExecuteMain();
        }
    }
}
