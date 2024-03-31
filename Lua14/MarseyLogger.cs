using System.Reflection;


public static class MarseyLogger
{
    // Информационные перечисления идентичны перечислениям в загрузчике, однако они не могут быть легко переведены из одного класса в другой
    public enum LogType
    {
        INFO,
        WARN,
        FATL,
        DEBG
    }

    // Делегат приводится загрузчиком к Marsey::Utility::Log(AssemblyName, string) во время выполнения
    public delegate void Forward(AssemblyName asm, string message);

    public static Forward? logDelegate;

    public static void Log(LogType type, string message)
    {
        logDelegate?.Invoke(Assembly.GetExecutingAssembly().GetName(), $"[{type.ToString()}] {message}");
    }
}