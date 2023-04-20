using YandexSpeechKitApi.Contracts;

namespace YandexSpeechKitApi.Clients;

public interface ISpeechKitClient
{
    Task<SpeechToTextResponse> SpeechToTextAsync(
        byte[] byteMedia,
        CancellationToken cancellationToken = default);
}