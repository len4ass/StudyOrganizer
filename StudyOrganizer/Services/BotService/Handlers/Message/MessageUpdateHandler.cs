using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService.Responses;
using Telegram.Bot;

namespace StudyOrganizer.Services.BotService.Handlers.Message;

public class MessageUpdateHandler : IUpdateHandler<Telegram.Bot.Types.Message>
{
    private readonly TextMessageUpdateHandler _textMessageUpdateHandler;
    private readonly VoiceMessageUpdateHandler _voiceMessageUpdateHandler;

    public MessageUpdateHandler(
        TextMessageUpdateHandler textMessageUpdateHandler,
        VoiceMessageUpdateHandler voiceMessageUpdateHandler)
    {
        _textMessageUpdateHandler = textMessageUpdateHandler;
        _voiceMessageUpdateHandler = voiceMessageUpdateHandler;
    }

    public async Task<BotResponse> HandleAsync(
        ITelegramBotClient client,
        Telegram.Bot.Types.Message update,
        UserInfo user)
    {
        return update.Type switch
        {
            Telegram.Bot.Types.Enums.MessageType.Voice => await _voiceMessageUpdateHandler.HandleAsync(
                client,
                update,
                user),
            Telegram.Bot.Types.Enums.MessageType.Text => await _textMessageUpdateHandler.HandleAsync(
                client,
                update,
                user),
            _ => throw new NotImplementedException()
        };
    }
}