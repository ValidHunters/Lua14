using Lua14.Data;
using Lua14.Lua;
using Robust.Shared.GameObjects;

namespace Lua14.Systems;

public sealed class LuaLoadingSystem : EntitySystem
{
    public override void Initialize()
    {
        var modsPath = LuaZipLoader.GetModsFolderPath();
        var mods = LuaZipLoader.ReadFolder(modsPath);
        foreach (var mod in mods)
        {
            LuaRunner runner = new(mod);
            runner.ExecuteMain();
        }
    }
}
