using Robust.Shared.IoC;
using Robust.Shared.Log;

namespace Lua14.Lua;

public class Logger(string loggerName = "LuaLog")
{
    private readonly ISawmill _sw = IoCManager.Resolve<ILogManager>().GetSawmill("lua." + loggerName);

    public void Verbose(string msg)
    {
        _sw.Verbose(msg);
    }

    public void Debug(string msg)
    {
        _sw.Debug(msg);
    }

    public void Info(string msg)
    {
        _sw.Info(msg);
    }

    public void Warning(string msg)
    {
        _sw.Warning(msg);
    }

    public void Error(string msg)
    {
        _sw.Error(msg);
    }

    public void Fatal(string msg)
    {
        _sw.Fatal(msg);
    }
}