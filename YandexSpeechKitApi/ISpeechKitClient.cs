namespace YandexSpeechKitApi;

public interface ISpeechKitClient
{
    Task<SpeechToTextResponse> SpeechToTextAsync(
        byte[] byteMedia,
        CancellationToken cancellationToken = default);
}