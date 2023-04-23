using OpenAI;
using OpenAI.Audio;
using Quartz.Util;

namespace StudyOrganizer.Services.OpenAi.SpeechToText;

public class OpenAiSpeechAnalyzer : IOpenAiSpeechAnalyzer
{
    private readonly OpenAIClient _api;

    public OpenAiSpeechAnalyzer(OpenAIClient api)
    {
        _api = api;
    }

    public async Task<OpenAiResponse> SpeechToTextAsync(Stream audio)
    {
        var result = await _api.AudioEndpoint.CreateTranscriptionAsync(
            new AudioTranscriptionRequest(
                audio,
                "transcribe",
                temperature: 0,
                language: "ru"));

        if (result.IsNullOrWhiteSpace())
        {
            return new OpenAiResponse
            {
                ResponseStatus = OpenAiResponseStatus.Failed
            };
        }

        return new OpenAiResponse
        {
            ResponseStatus = OpenAiResponseStatus.Ok,
            Result = result
        };
    }
}