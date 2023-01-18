using StudyOrganizer.Models;
using StudyOrganizer.Models.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Bot.Commands;

public interface IBotCommand
{
    public Task<string> Execute(
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments);
}