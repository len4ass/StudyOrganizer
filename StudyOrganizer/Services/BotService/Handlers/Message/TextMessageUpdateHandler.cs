using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Services.BotService.Command;
using StudyOrganizer.Services.BotService.Responses;
using Telegram.Bot;

namespace StudyOrganizer.Services.BotService.Handlers.Message;

public class TextMessageUpdateHandler : IUpdateHandler<Telegram.Bot.Types.Message>
{
    private readonly BotCommandAggregator _botCommandAggregator;

    public TextMessageUpdateHandler(BotCommandAggregator botCommandAggregator)
    {
        _botCommandAggregator = botCommandAggregator;
    }

    public async Task<BotResponse> HandleAsync(
        ITelegramBotClient client,
        Telegram.Bot.Types.Message update,
        UserInfo user)
    {
        var args = TextParser.ParseMessageToCommand(update.Text!);
        if (args.Count == 0)
        {
            return new BotResponse
            {
                User = user.Handle ?? user.Name
            };
        }

        return await _botCommandAggregator.ExecuteCommandByNameAsync(
            args[0],
            client,
            update,
            user,
            args.Skip(1)
                .ToList());
    }
}