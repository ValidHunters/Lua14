#if DEBUG

using Content.Shared.Administration;
using Lua14.Lua.Data.Structures;
using Robust.Shared.Console;

namespace Lua14.Lua.Commands
{
    [AnyCommand]
    internal sealed class LoadstringCommand : LocalizedCommands
    {
        public override string Command => "lua14.loadstring";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var mod = new Mod(
                new Config() { MainFile = "loadstring.lua", Name = "loadstring" },
                [new Chunk(
                    "loadstring.lua", args[0]
                )]
            );

            var runner = new Runner(mod);
            runner.ExecuteMain();
        }
    }
}

#endif