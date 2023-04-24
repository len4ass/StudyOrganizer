using System.Net;
using StudyOrganizer.Services.BotService.Responses;
using YandexSpeechKitApi.Clients;
using YandexSpeechKitApi.Contracts;

namespace StudyOrganizer.Services.YandexSpeechKit;

public class YandexSpeechAnalyzer
{
    private readonly ISpeechKitClient _speechKitClient;

    public YandexSpeechAnalyzer(ISpeechKitClient speechKitClient)
    {
        _speechKitClient = speechKitClient;
    }

    public async Task<UserResponse?> SpeechToText(byte[] audioBytes, CancellationToken cts)
    {
        var result = await _speechKitClient.SpeechToTextAsync(audioBytes, cts);
        if (result.TransportCode != HttpTransportCode.Ok || result.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        if (result.Result is null)
        {
            return null;
        }

        return new UserResponse(result.Result.Result);
    }
}