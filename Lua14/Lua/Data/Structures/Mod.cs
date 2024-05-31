using System.Diagnostics.CodeAnalysis;

namespace Lua14.Lua.Data.Structures;

public class Mod
{
    public Mod(Config config, Chunk[] chunks)
    {
        Config = config;
        Chunks = chunks;
    }

    public string Name => Config.Name;
    public string MainFile => Config.MainFile;

    public readonly Config Config;
    public readonly Chunk[] Chunks;

    public bool TryFindChunk(string path, [NotNullWhen(true)] out Chunk result)
    {
        foreach (var chunk in Chunks)
        {
            if (chunk.Path == path)
            {
                result = chunk;
                return true;
            }
        }

        result = default;
        return false;
    }
}
