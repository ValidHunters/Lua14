namespace Lua14.Mod;

internal sealed class LuaMod
{
    public LuaMod(LuaConfig config, List<LuaFile> entries)
    {
        Config = config;
        Entries = entries;
    }

    public LuaConfig Config;
    public List<LuaFile> Entries;
}
