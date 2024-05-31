using Lua14.Lua.Data.Structures;
using System.IO.Compression;

namespace Lua14.Lua.Data.Loaders;

public sealed class ZipModLoader : ModLoader
{
    public override Mod Load(string path)
    {
        Config? config = null;
        List<Chunk> chunks = [];

        ZipArchive zip = ZipFile.OpenRead(path);
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            if (entry.IsEncrypted)
                continue;

            var extension = Path.GetExtension(entry.Name);
            switch (extension)
            {
                case ".lua":
                    Stream chunkStream = entry.Open();
                    Chunk chunk = ReadChunk(chunkStream, entry.Name);
                    chunks.Add(chunk);
                    break;
                case ".json":
                    if (Path.GetFileNameWithoutExtension(entry.Name) != "config")
                        throw new Exception($"There was an another json file in the zip {path} (there should be only config.json)");

                    Stream configStream = entry.Open();
                    config = ReadConfig(configStream);
                    break;
                default:
                    throw new Exception($"There was a file with a wrong extension in the zip file {path} (there should be only .lua and .json files)");
            }
        }

        if (config == null)
            throw new Exception($"Unable to load config from {path}.");
        if (chunks.Count == 0)
            throw new Exception($"Zero lua chunks loaded from {path}.");

        return new Mod(config.Value, [.. chunks]);
    }
}
