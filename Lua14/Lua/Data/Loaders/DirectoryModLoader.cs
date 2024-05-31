using Lua14.Lua.Data.Structures;

namespace Lua14.Lua.Data.Loaders;

public sealed class DirectoryModLoader : ModLoader
{
    public override Mod Load(string path)
    {
        Config? config = null;
        List<Chunk> chunks = [];

        var files = Directory.EnumerateFiles(path);
        foreach (var file in files)
        {
            var extension = Path.GetExtension(file);
            switch (extension)
            {
                case ".lua":
                    Stream chunkStream = File.OpenRead(file);
                    var relativePath = GetRelativePath(path, file);
                    Chunk chunk = ReadChunk(chunkStream, relativePath);
                    chunks.Add(chunk);
                    break;
                case ".json":
                    if (Path.GetFileNameWithoutExtension(file) != "config")
                        throw new Exception($"There was an another json file in the folder {path} (there should be only config.json)");

                    Stream configStream = File.OpenRead(file);
                    config = ReadConfig(configStream);
                    break;
                default:
                    throw new Exception($"There was a file with a wrong extension in the folder {path} (there should be only .lua and .json files)");
            }
        }

        if (config == null)
            throw new Exception($"Unable to load config from {path}.");
        if (chunks.Count == 0)
            throw new Exception($"Zero lua chunks loaded from {path}.");

        return new Mod(config.Value, [.. chunks]);
    }

    private static string GetRelativePath(string rootPath, string fullPath)
    {
        return fullPath.Substring(rootPath.Length + 1);
    }
}
