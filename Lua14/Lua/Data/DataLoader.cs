using Lua14.Lua.Data.Loaders;
using Lua14.Lua.Data.Structures;
using System.Reflection;

namespace Lua14.Lua.Data;

public static class DataLoader
{
    private static readonly DirectoryModLoader _dirLoader = new();
    private static readonly ZipModLoader _zipLoader = new();

    private static string GetAutoexecFolderPath()
    {
        var currentAssemblyPath = Assembly.GetExecutingAssembly().Location;
        var marseyModsPath = Path.GetDirectoryName(currentAssemblyPath);
        var marseyPath = Path.GetDirectoryName(marseyModsPath) ?? throw new Exception("Wrong dll path");
        var luaAutoexecPath = Path.Combine(marseyPath, "LuaAutoexec");

        return luaAutoexecPath;
    }

    public static Mod[] ReadAutoexecFolder()
    {
        string path = GetAutoexecFolderPath();
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        List<Mod> mods = [];

        var zips = Directory.EnumerateFiles(path, "*.zip");
        foreach (var zip in zips)
        {
            Mod mod = _zipLoader.Load(zip);
            mods.Add(mod);
        }

        var directories = Directory.EnumerateDirectories(path);
        foreach (var directory in directories)
        {
            Mod mod = _dirLoader.Load(directory);
            mods.Add(mod);
        }

        return [..mods];
    }
}
