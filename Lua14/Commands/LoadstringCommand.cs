#if DEBUG

using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Lua14.Commands
{
    [AnyCommand]
    internal sealed class LoadstringCommand : LocalizedCommands
    {
        public override string Command => "lua.loadstring";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var lua = new NLua.Lua();
            var data = lua.DoString(args[0]);

            shell.WriteLine("OUTPUT");
            foreach (var item in data)
            {
                shell.WriteLine(item.ToString() ?? "null");
            }
            shell.WriteLine("END OUTPUT");
        }
    }
}

#endif