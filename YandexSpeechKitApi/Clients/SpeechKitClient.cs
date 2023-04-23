using System.Text.Json;
using YandexSpeechKitApi.Contracts;

namespace YandexSpeechKitApi.Clients;

public class SpeechKitClient : ISpeechKitClient
{
    private readonly Endpoints _endpoints;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public SpeechKitClient(
        string apiKey,
        Endpoints? commonUri = null)
    {
        _httpClient = new HttpClient();
        _apiKey = apiKey;

        _endpoints = commonUri ?? new Endpoints();
    }

    public async Task<SpeechToTextResponse> SpeechToTextAsync(
        byte[] byteMedia, 
        CancellationToken cancellationToken = default)
    {
        if (byteMedia == null)
        {
            throw new ArgumentNullException(nameof(byteMedia));
        }
        
        var queryParams = new Dictionary<string, string>
        {
            ["lang"] = "ru-RU"
        };

        var queryString = string.Join("&", queryParams.Select(pair => $"{pair.Key}={pair.Value}"));
        var uri = new Uri($"{_endpoints.SpeechToText}?{queryString}");
        var request = new HttpRequestMessage(HttpMethod.Post, uri);

        await using var memoryStream = new MemoryStream(byteMedia);
        request.Headers.Add("Authorization", $"Api-Key {_apiKey}");
        request.Content = new StreamContent(memoryStream);
        request.Content.Headers.Add("Content-Type", "application/octet-stream");
        if (request.Content.Headers.ContentLength > 1024 * 1024)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteMedia), 
                request.Content.Headers.ContentLength, 
                "Content length must be less than 1 MB.");
        }
        
        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new SpeechToTextResponse
                {
                    TransportCode = HttpTransportCode.Ok,
                    StatusCode = response.StatusCode,
                };
            }

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            return new SpeechToTextResponse
            {
                TransportCode = HttpTransportCode.Ok,
                StatusCode = response.StatusCode,
                Result = JsonSerializer.Deserialize<SpeechToTextResult>(jsonString)
            };
        }
        catch (HttpRequestException)
        {
            return new SpeechToTextResponse
            {
                TransportCode = HttpTransportCode.SocketError
            };
        }
    }
}