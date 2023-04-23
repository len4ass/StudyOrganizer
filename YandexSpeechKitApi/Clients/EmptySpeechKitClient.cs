using YandexSpeechKitApi.Contracts;

namespace YandexSpeechKitApi.Clients;

public class EmptySpeechKitClient : ISpeechKitClient
{
    public async Task<SpeechToTextResponse> SpeechToTextAsync(
        byte[] byteMedia, 
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new SpeechToTextResponse());
    }
}