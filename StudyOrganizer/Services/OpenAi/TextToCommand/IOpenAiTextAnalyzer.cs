namespace StudyOrganizer.Services.OpenAi.TextToCommand;

public interface IOpenAiTextAnalyzer
{
    Task<OpenAiResponse> TextToCommandAsync(string text);
}