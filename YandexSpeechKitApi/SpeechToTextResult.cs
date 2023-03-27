using System.Text.Json.Serialization;

namespace YandexSpeechKitApi;

public class SpeechToTextResult
{
    [JsonPropertyName("result")] 
    public string Result { get; set; } = default!;
}

