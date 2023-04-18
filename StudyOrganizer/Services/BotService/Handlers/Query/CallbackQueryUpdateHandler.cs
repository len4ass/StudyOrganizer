using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Services.BotService.Command;
using StudyOrganizer.Services.BotService.Handlers.Message;
using StudyOrganizer.Services.BotService.Responses;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Services.BotService.Handlers.Query;

public class CallbackQueryUpdateHandler : IUpdateHandler<CallbackQuery>
{
    private readonly BotCommandAggregator _botCommandAggregator;

    public CallbackQueryUpdateHandler(BotCommandAggregator botCommandAggregator)
    {
        _botCommandAggregator = botCommandAggregator;
    }

    public async Task<BotResponse> HandleAsync(
        ITelegramBotClient client,
        CallbackQuery update,
        UserInfo user)
    {
        var message = update.Message;
        var query = update.Data;

        var args = query!.Split();
        if (args.Length < 2)
        {
            return new BotResponse
            {
                User = user.Handle ?? user.Name,
                MessageType = MessageType.Query,
                InternalResponse = $"Ответ от пользователя {user.Handle ?? user.Name}) не является корректным."
            };
        }

        if (!long.TryParse(args[0], out var originalUserId))
        {
            return new BotResponse
            {
                User = user.Handle ?? user.Name,
                MessageType = MessageType.Query,
                InternalResponse = "Callback query не содержит id пользователя инициатора ответа первым аргументом."
            };
        }

        if (originalUserId != user.Id)
        {
            return new BotResponse
            {
                User = user.Handle ?? user.Name,
                MessageType = MessageType.Query,
                InternalResponse =
                    $"Ответ на query поступил не от инициатора операции: исходный id - {originalUserId}," +
                    $"полученный id - {user.Id}"
            };
        }

        var command = TextParser.ParseMessageToCommand(string.Join(' ', args.Skip(1)));
        return await _botCommandAggregator.ExecuteCommandByNameAsync(
            command[0],
            client,
            message!,
            user,
            command.Skip(1)
                .ToList());
    }
}