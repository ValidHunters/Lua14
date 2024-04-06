using Lua14.Data;
using NLua;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries;

public sealed class GlobalLibrary : LuaLibrary
{
    [Dependency] private readonly NLua.Lua _lua = default!;
    [Dependency] private readonly LuaLogger _logger = default!;
    [Dependency] private readonly LuaMod _mod = default!;

    public override string Name => "global";
    public override bool IsLibraryGlobal => true;

    private LuaTable _packageLoaded = default!;

    public override void Initialize()
    {
        _packageLoaded = _lua.GetTable("package.loaded");
    }

    [LuaMethod("print")]
    public void Print(params string[] values)
    {
        _logger.Debug(string.Concat(values));
    }

    [LuaMethod("require")]
    public dynamic Require(string path)
    {
        if (_packageLoaded[path] != null) return _packageLoaded[path];
        if (!_mod.TryFindFile(path, out var file))
            throw new Exception($"No file found with path {path}");

        _packageLoaded[path] = _lua.LoadString(file?.Content, "require_mod_chunk").Call();

        return _packageLoaded[path];
    }
}
