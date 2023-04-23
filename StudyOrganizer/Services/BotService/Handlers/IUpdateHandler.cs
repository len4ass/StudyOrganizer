using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService.Responses;
using Telegram.Bot;

namespace StudyOrganizer.Services.BotService.Handlers;

public interface IUpdateHandler<TUpdateType>
{
    Task<BotResponse> HandleAsync(
        ITelegramBotClient client,
        TUpdateType update,
        UserInfo user);
}