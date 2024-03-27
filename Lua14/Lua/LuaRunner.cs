using Lua14.Data;
using Robust.Shared.IoC;
using Robust.Shared.Reflection;

namespace Lua14.Lua;

public class LuaRunner
{
    [Dependency] private readonly IReflectionManager _reflection = default!;

    private readonly LuaMod _mod;
    private readonly LuaLogger _logger;
    private readonly NLua.Lua _state = new();
    private KeraLua.Lua _kstate => _state.State;
    public LuaRunner(LuaMod mod)
    {
        IoCManager.InjectDependencies(this);
        _mod = mod;
        _logger = new(mod.Config.Name);

        LoadLibs();
    }

    private void LoadLibs() {
        var libs = _reflection.GetAllChildren<LuaLibrary>();
        foreach (var lib in libs)
        {
            var library = (LuaLibrary)Activator.CreateInstance(lib, _state, _mod, _logger)!;
            library.Register();
        }
    }

    public object[] ExecuteMain() {
        if (!_mod.TryFindFile(_mod.Config.MainFile, out var file))
            throw new Exception($"No file found with path {_mod.Config.MainFile}");

        return _state.DoString(file.Content);
    }
}
