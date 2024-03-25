using Robust.Shared.ContentPack;
using Robust.Shared.IoC;

namespace Lua14;

public sealed class EntryPoint : GameShared
{
    public override void PostInit()
    {
        IoCManager.BuildGraph();
    }
}
