namespace StudyOrganizer;

internal static class Program
{
    public static async Task Main()
    {
        var runner = new ProgramRunner();
        await runner.Run();
    }
}