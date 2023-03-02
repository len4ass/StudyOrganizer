using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;

namespace StudyOrganizer;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var workingPaths = new WorkingPaths(
            args[0], 
            args[1], 
            args[2], 
            args[3], 
            args[4], 
            args[5]);

        var runner = new ProgramRunner(workingPaths);
        await runner.Run();
    }
}