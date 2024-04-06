using NLua;
using Robust.Shared.GameObjects;

namespace Lua14.Systems;

public class LuaSystem : EntitySystem
{
    private readonly Dictionary<string, Tuple<LuaFunction, LuaTable>> _updateFunctions = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        foreach (var (function, table) in _updateFunctions.Values)
        {
            function.Call(frameTime, table);
        }
    }

    public bool TryPutLuaSystem(string name, LuaFunction func, LuaTable table)
    {
        return _updateFunctions.TryAdd(name, new Tuple<LuaFunction, LuaTable>(func, table));
    }

    public bool TryRemoveLuaSystem(string name)
    {
        return _updateFunctions.Remove(name);
    }
}