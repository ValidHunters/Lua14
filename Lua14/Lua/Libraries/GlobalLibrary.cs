using Eluant;
using Lua14.Lua.Data.Structures;
using Lua14.Lua.Objects;
using Robust.Shared.IoC;

namespace Lua14.Lua.Libraries;

public sealed class GlobalLibrary(LuaRuntime lua) : Library(lua)
{
    [Dependency] private readonly Logger _logger = default!;
    [Dependency] private readonly Mod _mod = default!;

    protected override string Name => "global";
    protected override bool CreateGlobalTable => false;

    private readonly Dictionary<string, LuaVararg> _loaded = [];

    public override void Initialize()
    {
        using LuaFunction printFunc = Lua.CreateFunctionFromDelegate(Print);
        using LuaFunction requireFunc = Lua.CreateFunctionFromDelegate(Require);

        Lua.Globals["print"] = printFunc;
        Lua.Globals["require"] = requireFunc;
    }

    public void Print(LuaVararg vararg)
    {
        _logger.Debug(string.Join(" ", vararg));
    }

    public LuaVararg Require(string path)
    {
        if (_loaded.TryGetValue(path, out var value))
        {
            return value;
        }

        if (!_mod.TryFindChunk(path, out var chunk))
            throw new Exception($"No file found with path {path}");

        _loaded[path] = Lua.DoString(chunk.Content);
        return _loaded[path];
    }
}
