using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace ClearDeadlinesCommand;

public sealed class ClearDeadlinesCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public ClearDeadlinesCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "cleardeadlines";
        Description = "Удаляет все дедлайны из базы данных.";
        Format = "/cleardeadlines";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Normal
        };

        _dbContextFactory = dbContextFactory;
    }

    public override async Task<UserResponse> ExecuteAsync(
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments)
    {
        if (message.From!.IsBot && arguments.Count == 1 && arguments[0] == "Да")
        {
            var clearResponse = await ParseResponse(Array.Empty<string>());

            await BotMessager.EditMessage(
                client,
                message,
                clearResponse.Response);
            return clearResponse;
        }

        if (message.From!.IsBot && arguments.Count == 1 && arguments[0] == "Нет")
        {
            var clearResponse = new UserResponse("Удаление дедлайнов отменено.");

            await BotMessager.EditMessage(
                client,
                message,
                clearResponse.Response);
            return clearResponse;
        }

        var confirmationMarkup = new InlineKeyboardMarkup(
            new[]
            {
                new InlineKeyboardButton("Да")
                {
                    CallbackData = $"{userInfo.Id} /{Name} Да"
                },
                new InlineKeyboardButton("Нет")
                {
                    CallbackData = $"{userInfo.Id} /{Name} Нет"
                }
            });

        var userResponse = "Вы уверены, что хотите удалить все дедлайны?";
        await BotMessager.ReplyKeyboardMarkup(
            client,
            message,
            userResponse,
            confirmationMarkup);

        return UserResponseFactory.Success(Name);
    }

    private async Task<UserResponse> ParseResponse(IList<string> arguments)
    {
        if (arguments.Count > 0)
        {
            return UserResponseFactory.ArgumentLimitExceeded(Name);
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Deadlines");
        return UserResponseFactory.Success("Успешно удалены все дедлайны.");
    }
}