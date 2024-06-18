using Eluant;
using Lua14.Lua.Data.Structures;
using Robust.Shared.IoC;
using Robust.Shared.Reflection;

namespace Lua14.Lua.Objects;

public class Runner
{
    [Dependency] private readonly IReflectionManager _reflection = default!;
    [Dependency] private readonly IDependencyCollection _gameDeps = default!;

    private readonly IDependencyCollection _deps;

    private readonly Mod _mod;
    private readonly Logger _logger;
    private readonly LuaRuntime _runtime = new();

    public Runner(Mod mod)
    {
        IoCManager.InjectDependencies(this);
        _mod = mod;
        _logger = new(mod.Name);

        _deps = _gameDeps.FromParent(_gameDeps); // new DependencyCollection(_gameDeps)

        RegisterIoC();
        RegisterLibs();
        SetupSandbox();
    }

    private void SetupSandbox()
    {
        LuaGlobalsTable globals = _runtime.Globals;

        globals["io"] = null;
        globals["os"] = null;
        globals["debug"] = null;
        globals["luanet"] = null;
        globals["package"] = null;
        globals["dofile"] = null;
        globals["load"] = null;
    }

    private void RegisterIoC()
    {
        _deps.RegisterInstance<LuaRuntime>(_runtime);
        _deps.RegisterInstance<Mod>(_mod);
        _deps.RegisterInstance<Logger>(_logger);
    }

    private void RegisterLibs()
    {
        var libs = _reflection.GetAllChildren<Library>();

        foreach (var lib in libs)
        {
            _deps.Register(lib);
        }
        _deps.BuildGraph();

        foreach (var type in libs)
        {
            var library = (Library)_deps.ResolveType(type);

            library.Initialize();
            library.Register();
        }
    }

    public void ExecuteMain()
    {
        if (!_mod.TryFindChunk(_mod.MainFile, out var mainChunk))
            throw new Exception($"No file found with path {_mod.MainFile}");

        _runtime
            .DoString(mainChunk.Content)
            .Dispose();
    }
}
