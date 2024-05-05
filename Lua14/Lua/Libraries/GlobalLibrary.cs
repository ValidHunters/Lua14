using Lua14.Data;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries;

public sealed class GlobalLibrary : LuaLibrary
{
    [Dependency] private readonly NLua.Lua _lua = default!;
    [Dependency] private readonly LuaLogger _logger = default!;
    [Dependency] private readonly LuaMod _mod = default!;

    public override string Name => "global";
    public override bool IsLibraryGlobal => true;

    private readonly Dictionary<string, object[]> _loaded = [];

    [LuaMethod("print")]
    public void Print(params object[] values)
    {
        _logger.Debug(string.Join(" ", values));
    }

    [LuaMethod("require")]
    public object[] Require(string path)
    {
        if (_loaded.TryGetValue(path, out var value))
        {
            return value;
        }

        if (!_mod.TryFindFile(path, out var file))
            throw new Exception($"No file found with path {path}");

        _loaded[path] = _lua.DoString(file?.Content, "require_chunk");
        return _loaded[path];
    }
}
