namespace StudyOrganizer;

public static class GlobalCancellationToken
{
    public static CancellationTokenSource Cts { get; } = new();
}