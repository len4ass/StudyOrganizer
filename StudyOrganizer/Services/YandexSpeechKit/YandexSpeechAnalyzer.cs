using System.Net;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using YandexSpeechKitApi;

namespace StudyOrganizer.Services.YandexSpeechKit;

public class YandexSpeechAnalyzer
{
    private readonly ISpeechKitClient _speechKitClient;

    public YandexSpeechAnalyzer(ISpeechKitClient speechKitClient)
    {
        _speechKitClient = speechKitClient;
    }

    public async Task<UserResponse> SpeechToText(byte[] audioBytes, CancellationToken cts)
    {
        var result = await _speechKitClient.SpeechToTextAsync(audioBytes, cts);
        if (result.TransportCode != HttpTransportCode.Ok)
        {
            return new UserResponse
            {
                //InternalResponse =
                //$"Не удалось доставить голосовое сообщение пользователя {user.Handle} ({user.Id}) до сервера."
            };
        }

        if (result.StatusCode != HttpStatusCode.OK)
        {
            return new UserResponse
            {
                //InternalResponse =
                //$"При обработке голосового сообщения пользователя {user.Handle} ({user.Id}) произошла ошибка при получении ответа."
            };
        }

        if (result.Result is null)
        {
            return new UserResponse
            {
                //InternalResponse = $"Не удалось произвести обработку голосового сообщения {user.Handle} ({user.Id})."
            };
        }

        return new UserResponse
        {
            Response = result.Result.Result
        };
    }
}