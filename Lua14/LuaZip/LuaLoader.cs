using NLua;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;

namespace Lua14.Mod;

internal sealed class LuaLoader
{
    public void Initialize()
    {
        var currentAssemblyPath = Assembly.GetExecutingAssembly().Location;
        var marseyPath = Path.GetDirectoryName(currentAssemblyPath) ?? throw new Exception("Wrong dll location");
        var luaModsPath = Path.Combine(marseyPath, "LuaMods");

        InitFolder(luaModsPath);

        var lua = new Lua();
        lua.DoFile
    }

    private void InitFolder(string path)
    {
        var files = Directory.EnumerateFiles(path);
        foreach (var file in files)
        {
            var fileExtension = Path.GetExtension(file);
            if (fileExtension != ".zip")
                continue;

            ReadZip(file);
        }
    }
    private LuaMod ReadZip(string path)
    {
        ZipArchive archive = ZipFile.OpenRead(path);

        LuaConfig? config = null;
        List<LuaFile> entries = [];
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.IsEncrypted)
                continue;

            var extension = Path.GetExtension(entry.Name);
            switch(extension)
            {
                case ".lua":
                    Stream stream = entry.Open();
                    entries.Add(
                        ReadLuaFile(stream, entry.FullName)
                    );
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
            throw new Exception($"Unable to load config from ${path}");

        return new LuaMod(config, entries);
    }
    private LuaConfig ReadConfig(Stream configStream)
    {
        return JsonSerializer.Deserialize<LuaConfig>(configStream) ?? throw new Exception("Cant deserialize the json config file?");
    }
    private LuaFile ReadLuaFile(Stream fileStream, string filePath)
    {
        StreamReader reader = new StreamReader(fileStream);
        string content = reader.ReadToEnd();

        return new LuaFile(filePath, content);
    }
}
