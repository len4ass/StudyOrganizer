using Autofac;
using Serilog;
using StudyOrganizer.Hooks;
using StudyOrganizer.Settings;

namespace StudyOrganizer;

internal static class Program
{
    private static void InitializeExitHooks()
    {
        EventHook.AddMethodOnProcessExit((_, _) => Log.Logger.Information("Завершение работы."));
    }

    private static void CatchUnhandledExceptions()
    {
        EventHook.AddMethodOnUnhandledException(
            (_, args) => Log.Logger.Error(args.ExceptionObject as Exception, "Необработанное исключение!"));
    }

    private static void WaitTermination(CancellationTokenSource cts)
    {
        ManualResetEventSlim mre = new();
        Console.CancelKeyPress += (_, e) =>
        {
            mre.Set();
            e.Cancel = true;
        };
        mre.Wait();

        Log.Logger.Information("Caught CTRL+C, calling cancellation token.");
        cts.Cancel();
    }

    public static async Task Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Некорректное количество параметров командной строки.");
            return;
        }

        InitializeExitHooks();
        CatchUnhandledExceptions();
        var workingPaths = new WorkingPaths(
            Path.Combine(AppContext.BaseDirectory, args[0]),
            Path.Combine(AppContext.BaseDirectory, args[1]),
            Path.Combine(AppContext.BaseDirectory, args[2]),
            Path.Combine(AppContext.BaseDirectory, args[3]));

        var services = new Startup(workingPaths, new ContainerBuilder()).ConfigureServices();
        var cts = new CancellationTokenSource();
        foreach (var service in services)
        {
            await service.StartAsync(cts.Token);
        }

        WaitTermination(cts);
    }
}