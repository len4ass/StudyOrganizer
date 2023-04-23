namespace StudyOrganizer.Services.OpenAi.SpeechToText;

public interface IOpenAiSpeechAnalyzer
{
    Task<OpenAiResponse> SpeechToTextAsync(Stream audio);
}