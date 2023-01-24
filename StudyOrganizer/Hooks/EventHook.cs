namespace StudyOrganizer.Hooks;

public static class EventHook
{
    public static void AddMethodOnProcessExit(Action<object?, EventArgs> function)
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(function);
    }

    public static void AddMethodOnUnhandledException(Action<object?, UnhandledExceptionEventArgs> function)
    {
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(function);
    }
}