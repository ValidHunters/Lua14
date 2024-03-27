using System.Diagnostics.CodeAnalysis;

namespace Lua14.Data;

public sealed class LuaMod
{
    public readonly LuaConfig Config;
    public readonly List<LuaFile> Entries;

    public LuaMod(LuaConfig config, List<LuaFile> entries)
    {
        Config = config;
        Entries = entries;
    }

    public bool TryFindFile(string relativePath, [NotNullWhen(true)] out LuaFile? file)
    {
        foreach (var entry in Entries)
        {
            if (entry.RelativePath == relativePath)
            {
                file = entry;
                return true;
            }
        }

        file = null;
        return false;
    }
}
