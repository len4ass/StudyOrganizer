namespace StudyOrganizer.Services.OpenAi;

public class EmptyTextAnalyzer : IOpenAiTextAnalyzer
{
    public async Task<OpenAiResponse> TextToCommandAsync(string text)
    {
        return await Task.FromResult(
            new OpenAiResponse
            {
                ResponseStatus = OpenAiResponseStatus.Unsupported
            });
    }
}