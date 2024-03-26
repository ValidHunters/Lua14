namespace Lua14.Loader;

using NLua;
using System.Reflection;

internal sealed class LuaMod
{
    public readonly LuaConfig Config;
    public readonly List<LuaFile> Entries;

    private readonly Lua _state = new();
    private KeraLua.Lua _cstate => _state.State;

    public LuaMod(LuaConfig config, List<LuaFile> entries)
    {
        Config = config;
        Entries = entries;

        SetupState();
    }

    public void ExecuteMain()
    {
        if (!TryFindFile(Config.MainFile, out var file))
            throw new Exception($"No file found with path {Config.MainFile}");

        _state.DoString(file?.Content);
    }

    private void SetupState()
    {
        _state.RegisterFunction("require_mod", GetType().GetMethod("RequireModFunc", BindingFlags.NonPublic));
    }
    private object RequireModFunc(string path)
    {
        var loaded = _state.GetTable("package.loaded");
        if (loaded[path] != null) return loaded[path];
        if (!TryFindFile(path, out var file))
            throw new Exception($"No file found with path {path}");

        loaded[path] = _state.LoadString(file?.Content, "require_mod_chunk").Call();

        return loaded[path];
    }


    private bool TryFindFile(string relativePath, out LuaFile? file)
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
