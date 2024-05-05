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
    public object? Require(string path)
    {
        if (_loaded.TryGetValue(path, out var value))
        {
            // this is some goofy stuff to allow lua tuples
            // todo make it better by patching NLua.ObjectTranslator.PushMultiple
            if (value.Length > 6)
                return (value[0], value[1], value[2], value[3], value[4], value[5], value[6]);
            else if (value.Length > 5)
                return (value[0], value[1], value[2], value[3], value[4], value[5]);
            else if (value.Length > 4)
                return (value[0], value[1], value[2], value[3], value[4]);
            else if (value.Length > 3)
                return (value[0], value[1], value[2], value[3]);
            else if (value.Length > 2)
                return (value[0], value[1], value[2]);
            else if (value.Length > 1)
                return (value[0], value[1]);
            else if (value.Length > 0)
                return value[0];
            else
                return null;
        }

        if (!_mod.TryFindFile(path, out var file))
            throw new Exception($"No file found with path {path}");

        _loaded[path] = _lua.DoString(file?.Content, "require_chunk");
        return Require(path);
    }
}
