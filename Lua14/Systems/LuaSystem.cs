using NLua;
using Robust.Shared.GameObjects;

namespace Lua14.Systems;

public class LuaSystem : EntitySystem
{
    private readonly Dictionary<string, Tuple<LuaFunction?, LuaFunction?, LuaTable>> _functions = new();

    public override void Initialize()
    {
        base.Initialize();
        foreach (var (function, _, table) in _functions.Values)
        {
            function?.Call(table);
        }
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        foreach (var (_, function, table) in _functions.Values)
        {
            function?.Call(frameTime, table);
        }
    }

    public bool TryPutLuaSystem(string name, LuaFunction? funcInit, LuaFunction? funcUpdate, LuaTable table)
    {
        return _functions.TryAdd(name, new Tuple<LuaFunction?, LuaFunction?, LuaTable>(funcInit, funcUpdate, table));
    }

    public bool TryRemoveLuaSystem(string name)
    {
        return _functions.Remove(name);
    }
}