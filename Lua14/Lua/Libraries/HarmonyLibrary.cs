using Lua14.Data;

namespace Lua14.Lua.Libraries;

public class HarmonyLibrary(NLua.Lua lua, LuaMod mod, LuaLogger log) : LuaLibrary(lua, mod, log)
{
    public override string Name => "harmony";
}
