using Lua14.Lua.Data.Structures;
using System.Text.Json;

namespace Lua14.Lua.Data;

public abstract class ModLoader
{
    public abstract Mod Load(string path);

    public Config ReadConfig(Stream stream)
    {
        return JsonSerializer.Deserialize<Config>(stream);
    }
    public Chunk ReadChunk(Stream stream, string path)
    {
        StreamReader reader = new(stream);
        string content = reader.ReadToEnd();

        return new (path, content);
    }
}
