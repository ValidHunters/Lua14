using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using Robust.Shared.ContentPack;
using Robust.Shared.Log;

namespace Lua14.Lua.Data;

public static class LuaDataLoader
{
    private static readonly Type _packLoader = AccessTools.TypeByName("Robust.Shared.ContentPack.ResourceManager.PackLoader");
    private static readonly ISawmill _sawmill = Logger.GetSawmill("lua.data");

    public static string GetModsFolderPath()
    {
        var currentAssemblyPath = Assembly.GetExecutingAssembly().Location;
        var marseyModsPath = Path.GetDirectoryName(currentAssemblyPath);
        var marseyPath = Path.GetDirectoryName(marseyModsPath) ?? throw new Exception("Wrong dll path");
        var luaModsPath = Path.Combine(marseyPath, "LuaMods");

        return luaModsPath;
    }

    public static IContentRoot GetContentRoot(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        string extension = Path.GetExtension(path);

        if (extension == string.Empty)
            return CreateDirectoryContentRoot(path);
        if (extension == ".zip")
            return CreateZipContentRoot(path);
        
        throw new Exception($"Extension '{extension}' is not supported.");
    }

    public static IContentRoot CreateDirectoryContentRoot(string path) 
    {
        new PackLoader();
    }

    public static IContentRoot CreateZipContentRoot(string path) 
    {
        FileInfo info = new(path);
        return (IContentRoot)Activator.CreateInstance(_packLoader, [info, _sawmill]);
    }
}
