using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace PingCommand;

public sealed class PingCommand : BotCommand
{
    public PingCommand()
    {
        Name = "ping";
        Description = "Пингует на сервер.";
        Format = "/ping";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Normal
        };
    }

    public override async Task<UserResponse> ExecuteAsync(
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments)
    {
        var response = $"Ponged in chat: {message.Chat.Id}";

        await BotMessager.Reply(
            client,
            message,
            response);
        return new UserResponse(response);
    }
}