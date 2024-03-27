using Lua14.Data;

namespace Lua14.Lua.Libraries;

public class GlobalLibrary(NLua.Lua lua, LuaMod mod, LuaLogger log) : LuaLibrary(lua, mod, log)
{
    public override string Name => "global";

    public override bool IsLibraryGlobal => true;

    [LuaMethod("print")]
    void Print(params string[] values)
    {
        Logger.Debug(string.Concat(values));
    }

    [LuaMethod("require")]
    object Require(string path)
    {
        var loaded = Lua.GetTable("package.loaded");
        if (loaded[path] != null) return loaded[path];
        if (!Mod.TryFindFile(path, out var file))
            throw new Exception($"No file found with path {path}");

        loaded[path] = Lua.LoadString(file?.Content, "require_mod_chunk").Call();

        return loaded[path];
    }
}
