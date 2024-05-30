using Lua14.Lua.Data;
using NLua;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries;

public sealed class GlobalLibrary(NLua.Lua lua) : LuaLibrary(lua)
{
    [Dependency] private readonly LuaLogger _logger = default!;
    [Dependency] private readonly LuaMod _mod = default!;

    protected override string Name => "global";
    protected override bool CreateGlobalTable => false;

    private readonly Dictionary<string, object[]> _loaded = [];

    public override void Initialize()
    {
        Lua["print"] = Userdata["print"];
        Lua["require"] = Userdata["require"];
    }

    [LuaMember(Name = "print")]
    public void Print(params object[] values)
    {
        _logger.Debug(string.Join(" ", values));
    }

    [LuaMember(Name = "require")]
    public object[] Require(string path)
    {
        if (_loaded.TryGetValue(path, out var value))
        {
            return value;
        }

        if (!_mod.TryFindFile(path, out var file))
            throw new Exception($"No file found with path {path}");

        _loaded[path] = Lua.DoString(file?.Content, "require_chunk");
        return _loaded[path];
    }
}
