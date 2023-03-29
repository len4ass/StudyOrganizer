using Autofac;
using Serilog;
using StudyOrganizer.Hooks;
using StudyOrganizer.Settings;

namespace StudyOrganizer;

internal static class Program
{
    private static void InitializeExitHooks()
    {
        EventHook.AddMethodOnProcessExit((_, _) =>
        {
            Log.Logger.Information("Завершение работы.");
        });
    }
    
    private static void CatchUnhandledExceptions()
    {
        EventHook.AddMethodOnUnhandledException((_, args) =>
        {
            Log.Logger.Error(args.ExceptionObject as Exception, "Необработанное исключение!");
        });
    }
    
    public static async Task Main(string[] args)
    {
        if (args.Length != 9)
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
            Path.Combine(AppContext.BaseDirectory, args[3]), 
            Path.Combine(AppContext.BaseDirectory, args[4]), 
            Path.Combine(AppContext.BaseDirectory, args[5]),
            Path.Combine(AppContext.BaseDirectory, args[6]), 
            Path.Combine(AppContext.BaseDirectory, args[7]),
            Path.Combine(AppContext.BaseDirectory, args[8]));

        var services = new Startup(workingPaths, new ContainerBuilder()).ConfigureServices();
        var cts = new CancellationTokenSource();
        foreach (var service in services)
        {
            await service.StartAsync(cts.Token);
        }

        Console.ReadLine();
        cts.Cancel();
    }
}