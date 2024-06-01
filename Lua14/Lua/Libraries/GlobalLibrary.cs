using Lua14.Lua.Data.Structures;
using NLua;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries;

public sealed class GlobalLibrary(NLua.Lua lua) : Library(lua)
{
    [Dependency] private readonly Logger _logger = default!;
    [Dependency] private readonly Mod _mod = default!;

    protected override string Name => "global";
    protected override bool CreateGlobalTable => false;

    private readonly Dictionary<string, object[]> _loaded = [];

    public override void Initialize()
    {
        Lua["print"] = this["print"];
        Lua["require"] = this["require"];
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

        if (!_mod.TryFindChunk(path, out var chunk))
            throw new Exception($"No file found with path {path}");

        _loaded[path] = Lua.DoString(chunk.Content, "require_chunk");
        return _loaded[path];
    }
}
