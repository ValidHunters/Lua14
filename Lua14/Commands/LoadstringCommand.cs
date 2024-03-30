#if DEBUG

using Content.Shared.Administration;
using Lua14.Data;
using Lua14.Lua;
using Robust.Shared.Console;

namespace Lua14.Commands
{
    [AnyCommand]
    internal sealed class LoadstringCommand : LocalizedCommands
    {
        public override string Command => "lua14.loadstring";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var mod = new LuaMod(
                new LuaConfig() { MainFile = "loadstring.lua", Name = "loadstring" },
                [new LuaFile(
                    "loadstring.lua", args[0]
                )]
            );

            var runner = new LuaRunner(mod);
            runner.ExecuteMain();
        }
    }
}

#endif