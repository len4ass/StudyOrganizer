using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace RemoveUserCommand;

public sealed class RemoveUserCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public RemoveUserCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "removeuser";
        Description = "Удаляет пользователя из базы данных по его идентификатору или хэндлу.";
        Format = "/removeuser <id/handle>";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Owner
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

        if (arguments.Count == 1)
        {
            var parsedId = long.TryParse(arguments[0], out var id);
            if (!parsedId)
            {
                var handle = arguments[0]
                    .Replace("@", "");
                return await RemoveUserFromDatabaseByHandle(handle);
            }

            return await RemoveUserFromDatabaseById(id);
        }

        return UserResponseFactory.ArgumentLimitExceeded(Name);
    }

    private async Task<UserResponse> RemoveUserFromDatabaseById(long id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return UserResponseFactory.UserDoesNotExist(Name, id.ToString());
        }

        var userId = user.Id;
        var userHandle = user.Handle ?? user.Name;

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();

        var userResponse =
            $"Пользователь <b>{userHandle}</b> ({userId}) успешно удален из базы данных.";

        return UserResponseFactory.Success(userResponse);
    }

    private async Task<UserResponse> RemoveUserFromDatabaseByHandle(string handle)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Handle == handle);
        if (user is null)
        {
            return UserResponseFactory.UserDoesNotExist(Name, handle);
        }

        var userId = user.Id;
        var userHandle = user.Handle ?? user.Name;
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();

        var userResponse =
            $"Пользователь <b>{userHandle}</b> ({userId}) успешно удален из базы данных.";

        return UserResponseFactory.Success(userResponse);
    }
}