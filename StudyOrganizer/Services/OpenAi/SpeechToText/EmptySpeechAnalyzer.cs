namespace StudyOrganizer.Services.OpenAi.SpeechToText;

public class EmptySpeechAnalyzer : IOpenAiSpeechAnalyzer
{
    public async Task<OpenAiResponse> SpeechToTextAsync(Stream audio)
    {
        return await Task.FromResult(
            new OpenAiResponse
            {
                ResponseStatus = OpenAiResponseStatus.Unsupported
            });
    }
}