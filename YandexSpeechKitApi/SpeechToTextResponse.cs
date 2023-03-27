using System.Net;

namespace YandexSpeechKitApi;

public class SpeechToTextResponse
{
    public HttpTransportCode TransportCode { get; init; }
    public HttpStatusCode StatusCode { get; init; }
    public SpeechToTextResult? Result { get; init; }
}