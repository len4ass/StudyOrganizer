using System.Net;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using YandexSpeechKitApi;

namespace StudyOrganizer.Services.YandexSpeechKit;

public class YandexSpeechAnalyzer
{
    private readonly ISpeechKitClient _speechKitClient;

    public YandexSpeechAnalyzer(ISpeechKitClient speechKitClient)
    {
        _speechKitClient = speechKitClient;
    }

    public async Task<BotResponse> SpeechToText(
        byte[] audioBytes, 
        UserInfo user, 
        CancellationToken cts)
    {
        var result = await _speechKitClient.SpeechToTextAsync(audioBytes, cts);
        if (result.TransportCode != HttpTransportCode.Ok)
        {
            return new BotResponse("",
                $"Не удалось доставить голосовое сообщение пользователя {user.Handle} ({user.Id}) до сервера.");
        }
        
        if (result.StatusCode != HttpStatusCode.OK)
        {
            return new BotResponse("",
                $"При обработке голосового сообщения пользователя {user.Handle} ({user.Id}) произошла ошибка при получении ответа.");
        }

        if (result.Result is null)
        {
            return new BotResponse("",
                $"Не удалось произвести обработку голосового сообщения {user.Handle} ({user.Id}).");
        }

        return new BotResponse(result.Result.Result, 
            $"Успешно обработано сообщение пользователя ({user.Handle}) ({user.Id})");
    }
}