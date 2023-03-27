namespace StudyOrganizer.Services.OpenAi;

public class OpenAiResponse
{
    public OpenAiResponseStatus ResponseStatus { get; init; }
    public string Result { get; init; } = default!;
}