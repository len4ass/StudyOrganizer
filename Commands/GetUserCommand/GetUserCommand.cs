using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace GetUserCommand;

public sealed class GetUserCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public GetUserCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "getuser";
        Description = "Ищет пользователя в базе данных по его идентификатору/юзернейму и выводит информацию о нем.";
        Format = "/getuser <id/username>";
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

        if (arguments.Count == 1)
        {
            return await GetUser(arguments);
        }

        return UserResponseFactory.ArgumentLimitExceeded(Name);
    }

    private async Task<UserResponse> GetUser(IList<string> arguments)
    {
        var user = arguments[0]
            .Trim('@');

        var isId = long.TryParse(user, out var userId);
        if (isId)
        {
            return await GetUserById(userId);
        }

        return await GetUserByHandle(user);
    }

    private async Task<UserResponse> GetUserById(long id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var userRepository = new UserInfoRepository(dbContext);

        var user = await userRepository.FindAsync(id);
        if (user is null)
        {
            return UserResponseFactory.EntryDoesNotExist(
                Name,
                "пользователь с id",
                id.ToString());
        }

        return UserResponseFactory.Success(user.ToString());
    }

    private async Task<UserResponse> GetUserByHandle(string handle)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var userRepository = new UserInfoRepository(dbContext);

        var user = await userRepository.FindAsync(handle);
        if (user is null)
        {
            return UserResponseFactory.EntryDoesNotExist(
                Name,
                "пользователь с хэндлом",
                handle);
        }

        return UserResponseFactory.Success(user.ToString());
    }
}