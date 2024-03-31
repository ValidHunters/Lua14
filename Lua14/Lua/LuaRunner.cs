using Lua14.Data;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Reflection;

namespace Lua14.Lua;

public class LuaRunner
{
    [Dependency] private readonly IReflectionManager _reflection = default!;

    public readonly LuaMod Mod;
    private readonly LuaLogger _logger;
    private readonly NLua.Lua _state = new();
    private readonly IDependencyCollection _deps;

    public LuaRunner(LuaMod mod)
    {
        IoCManager.InjectDependencies(this);
        Mod = mod;
        _logger = new(mod.Config.Name);

        Type depsType = _reflection.GetType("IoC.DependencyCollection")!;
        _deps = (IDependencyCollection)Activator.CreateInstance(depsType)!;

        RegisterLibs();
        LoadLibs();
    }

    private void RegisterLibs() {
        var libs = _reflection.GetAllChildren<LuaLibrary>();
        foreach (var lib in libs)
        {
            var library = (LuaLibrary)Activator.CreateInstance(lib, _state, Mod, _logger)!;

            _deps.RegisterInstance(lib, library);
        }
        _deps.BuildGraph();
    }

    private void LoadLibs() {
        foreach (var type in _deps.GetRegisteredTypes())
        {
            var library = (LuaLibrary)_deps.ResolveType(type);
            _deps.InjectDependencies(library);

            library.Initialize();
            library.Register();
        }
    }

    public object[] ExecuteMain() {
        if (!Mod.TryFindFile(Mod.Config.MainFile, out var file))
            throw new Exception($"No file found with path {Mod.Config.MainFile}");

        return _state.DoString(file.Content, Mod.Config.Name);
    }
}
