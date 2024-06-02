using Lua14.Lua.Data.Structures;
using NLua.Exceptions;
using Robust.Shared.IoC;
using Robust.Shared.Reflection;

namespace Lua14.Lua;

public class Runner
{
    [Dependency] private readonly IReflectionManager _reflection = default!;
    [Dependency] private readonly IDependencyCollection _gameDeps = default!;

    private readonly IDependencyCollection _deps;
    private readonly List<Type> _librariesTypes = [];

    private readonly Mod _mod;
    private readonly Logger _logger;
    private readonly NLua.Lua _state = new();

    public Runner(Mod mod)
    {
        IoCManager.InjectDependencies(this);
        _mod = mod;
        _logger = new(mod.Name);

        _deps = _gameDeps.FromParent(_gameDeps); // new DependencyCollection(_gameDeps)

        RegisterIoC();
        RegisterLibs();
        LoadLibs();
        SetupSandbox();
    }

    private void SetupSandbox()
    {
        _state["io"] = null;
        _state["os"] = null;
        _state["debug"] = null;
        _state["luanet"] = null;
        _state["package"] = null;
        _state["dofile"] = null;
        _state["load"] = null;
    }

    private void RegisterIoC()
    {
        _deps.RegisterInstance<NLua.Lua>(_state);
        _deps.RegisterInstance<Mod>(_mod);
        _deps.RegisterInstance<Logger>(_logger);
    }

    private void RegisterLibs() {
        var libs = _reflection.GetAllChildren<Library>();
        _librariesTypes.AddRange(libs);

        foreach (var lib in libs)
        {
            _deps.Register(lib);
        }
        _deps.BuildGraph();
    }

    private void LoadLibs() {
        foreach (var type in _librariesTypes)
        {
            var library = (Library)_deps.ResolveType(type);

            library.Initialize();
            library.Register();
        }
    }

    public object[] ExecuteMain()
    {
        if (!_mod.TryFindChunk(_mod.MainFile, out var mainChunk))
            throw new Exception($"No file found with path {_mod.MainFile}");

        try
        {
            return _state.DoString(mainChunk.Content, _mod.Name);
        }
        catch (LuaScriptException ex)
        {
            if (ex.IsNetException && ex.InnerException != null)
            {
                _logger.Error($"Error while executing a mod with name {_mod.Name}. Source: ${ex.Source}");
                throw ex.InnerException;
            }
            throw;
        }
    }
}
