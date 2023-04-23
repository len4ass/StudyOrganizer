using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace RemoveDeadlineCommand;

public sealed class RemoveDeadlineCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public RemoveDeadlineCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "removedeadline";
        Description = "Удаляет дедлайн из базы данных по его названию.";
        Format = "/removedeadline <name>";
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
        var response = await ParseResponse(arguments);
        await BotMessager.Reply(
            client,
            message,
            response.Response);

        return response;
    }

    private async Task<UserResponse> ParseResponse(IList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return UserResponseFactory.NotEnoughArguments(Name);
        }

        if (arguments.Count > 1)
        {
            return UserResponseFactory.ArgumentLimitExceeded(Name);
        }

        return await DeleteDeadlineByName(arguments[0]);
    }

    private async Task<UserResponse> DeleteDeadlineByName(string deadlineName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deadline = await dbContext.Deadlines.FindAsync(deadlineName);
        if (deadline is null)
        {
            return UserResponseFactory.DeadlineDoesNotExist(Name, deadlineName);
        }

        dbContext.Deadlines.Remove(deadline);
        await dbContext.SaveChangesAsync();

        var userResponse = $"Дедлайн <b>{deadline.Name}</b> успешно удален из базы данных.";
        return UserResponseFactory.Success(userResponse);
    }
}