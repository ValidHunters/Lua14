using Eluant;
using Eluant.ObjectBinding;
using Lua14.Lua.Objects;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Reflection;
using Robust.Shared.Utility;
using System.Reflection;

namespace Lua14.Lua.Libraries;

public sealed class FileSystemLibrary(LuaRuntime lua) : Library(lua)
{
    [Dependency] private readonly IReflectionManager _reflection = default!;
    private IWritableDirProvider Data = default!;

    protected override string Name => "fs";

    private static string GetDataFolderPath()
    {
        var currentAssemblyPath = Assembly.GetExecutingAssembly().Location;
        var marseyModsPath = Path.GetDirectoryName(currentAssemblyPath);
        var marseyPath = Path.GetDirectoryName(marseyModsPath) ?? throw new Exception("Wrong dll path");
        var luaDataPath = Path.Combine(marseyPath, "LuaData");

        return luaDataPath;
    }

    public override void Initialize()
    {
        Data = (IWritableDirProvider) Activator.CreateInstance(
            _reflection.GetType("Robust.Shared.ContentPack.WritableDirProvider")!,
            Directory.CreateDirectory(GetDataFolderPath())
        )!;
    }

    [LuaMember("readfile")]
    public string ReadFile(string path) => Data.ReadAllText(new ResPath(path));

    [LuaMember("writefile")]
    public void WriteFile(string path, string content) => Data.WriteAllText(new ResPath(path), content);

    [LuaMember("appendfile")]
    public void AppendFile(string path, string content) => Data.AppendAllText(new ResPath(path), content);

    [LuaMember("delete")]
    public void Delete(string path) => Data.Delete(new ResPath(path));

    [LuaMember("makefolder")]
    public void MakeFolder(string path) => Data.CreateDir(new ResPath(path));

    [LuaMember("isfolder")]
    public bool IsFolder(string path) => Data.IsDir(new ResPath(path));

    [LuaMember("isfile")]
    public bool IsFile(string path) => !IsFolder(path);

    [LuaMember("listfiles")]
    public LuaValue ListFiles(string path)
    {
        var entries = Data.DirectoryEntries(new ResPath(path));

        return entries.ToLuaValue(Lua);
    }
}
