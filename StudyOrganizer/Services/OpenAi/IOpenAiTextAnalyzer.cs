namespace StudyOrganizer.Services.OpenAi;

public interface IOpenAiTextAnalyzer
{
    Task<OpenAiResponse> TextToCommandAsync(string text);
}