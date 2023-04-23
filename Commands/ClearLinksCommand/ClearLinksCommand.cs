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

namespace ClearLinksCommand;

public sealed class ClearLinksCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public ClearLinksCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "clearlinks";
        Description = "Удаляет все ссылки из базы данных.";
        Format = "/clearlinks";
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
            var clearResponse = new UserResponse("Удаление ссылок отменено.");

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

        var userResponse = "Вы уверены, что хотите удалить все ссылки?";
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
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Links");
        return UserResponseFactory.Success("Успешно удалены все ссылки.");
    }
}