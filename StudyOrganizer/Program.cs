using Autofac;
using StudyOrganizer.Bot.Commands.Normal;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;

namespace StudyOrganizer;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var workingPaths = new WorkingPaths(
            Path.Combine(AppContext.BaseDirectory, args[0]),
            Path.Combine(AppContext.BaseDirectory, args[1]), 
            Path.Combine(AppContext.BaseDirectory, args[2]), 
            Path.Combine(AppContext.BaseDirectory, args[3]), 
            Path.Combine(AppContext.BaseDirectory, args[4]), 
            Path.Combine(AppContext.BaseDirectory, args[5]));

        /*var runner = new ProgramRunner(workingPaths);
        await runner.Run(new CancellationTokenSource().Token);*/
        
        //
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