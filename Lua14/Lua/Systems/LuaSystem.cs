using Eluant;
using Robust.Shared.GameObjects;

namespace Lua14.Lua.Systems;

public class LuaSystem : EntitySystem
{
    private readonly HashSet<LuaSystemTable> _systems = [];

    public override void Shutdown()
    {
        base.Shutdown();
        foreach (var table in _systems)
        {
            table.Shutdown?.Call();
            table.Dispose();
        }

        _systems.Clear();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        foreach (var table in _systems)
        {
            table.Update?.Call(frameTime);
        }
    }

    public void PutLuaSystem(LuaSystemTable systemTable)
    {
        if (_systems.Where(it => it.Id == systemTable.Id).Any())
            throw new Exception("There is already a system with this id registred.");

        systemTable.Initialize?.Call();
        _systems.Add(systemTable);
    }

    public void RemoveLuaSystem(string id)
    {
        _systems.RemoveWhere(it => it.Id == id);
    }
}

public struct LuaSystemTable : IDisposable
{
    public string Id;
    public LuaFunction Initialize;
    public LuaFunction Update;
    public LuaFunction Shutdown;

    public void Dispose()
    {
        Initialize.Dispose();
        Update.Dispose();
        Shutdown.Dispose();
    }
}