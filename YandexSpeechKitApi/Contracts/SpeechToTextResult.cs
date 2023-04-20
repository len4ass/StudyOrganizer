using System.Text.Json.Serialization;

namespace YandexSpeechKitApi.Contracts;

public class SpeechToTextResult
{
    [JsonPropertyName("result")] 
    public string Result { get; set; } = default!;
}

