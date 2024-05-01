using NLua;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Reflection;
using Robust.Shared.Utility;
using System.Reflection;

namespace Lua14.Lua.Libraries;

public sealed class FileSystemLibrary : LuaLibrary
{
    [Dependency] private readonly IReflectionManager _reflection = default!;
    [Dependency] private readonly NLua.Lua _lua = default!;
    public IWritableDirProvider Data = default!;

    public override string Name => "fs";

    public static string GetDataFolderPath()
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
            GetDataFolderPath()
        )!;
    }

    [LuaMethod("readfile")]
    public string ReadFile(string path)
    {
        return Data.ReadAllText(new ResPath(path));
    }

    [LuaMethod("writefile")]
    public void WriteFile(string path, string content)
    {
        Data.WriteAllText(new ResPath(path), content);
    }

    [LuaMethod("appendfile")]
    public void AppendFile(string path, string content)
    {
        Data.AppendAllText(new ResPath(path), content);
    }

    [LuaMethod("delete")]
    public void Delete(string path)
    {
        Data.Delete(new ResPath(path));
    }

    [LuaMethod("makefolder")]
    public void MakeFolder(string path)
    {
        Data.CreateDir(new ResPath(path));
    }

    [LuaMethod("isfolder")]
    public bool IsFolder(string path)
    {
        return Data.IsDir(new ResPath(path));
    }

    [LuaMethod("isfile")]
    public bool IsFile(string path)
    {
        return !IsFolder(path);
    }

    [LuaMethod("listfiles")]
    public LuaTable ListFiles(string path)
    {
        var entries = Data.DirectoryEntries(new ResPath(path));

        return _lua.EnumerableToTable(entries);
    }
}
