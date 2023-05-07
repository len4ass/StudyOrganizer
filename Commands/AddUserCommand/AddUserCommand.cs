using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace AddUserCommand;

public sealed class AddUserCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public AddUserCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "adduser";
        Description = "Добавляет пользователя в базу данных по его идентификатору.";
        Format = "/adduser <id>";
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
        var response = await ParseResponse(
            client,
            message,
            arguments);

        await BotMessager.Reply(
            client,
            message,
            response.Response);
        return response;
    }

    private async Task<UserResponse> ParseResponse(
        ITelegramBotClient client,
        Message message,
        IList<string> arguments)
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
                UserResponseFactory.FailedParsingSpecified(Name, "Не удалось получить id пользователя.");
            }

            return await AddUserToDatabase(
                client,
                message,
                id);
        }

        return UserResponseFactory.ArgumentLimitExceeded(Name);
    }

    private async Task<UserResponse> AddUserToDatabase(
        ITelegramBotClient client,
        Message message,
        long id)
    {
        try
        {
            var chatMember = await client.GetChatMemberAsync(message.Chat, id);
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var user = await dbContext.Users.FindAsync(id);
            if (user is not null)
            {
                return UserResponseFactory.UserAlreadyExists(Name, user.Handle ?? user.Name);
            }

            var newUser = new UserInfo
            {
                Id = chatMember.User.Id,
                Handle = chatMember.User.Username,
                Name = chatMember.User.FirstName,
                Level = AccessLevel.Normal
            };

            await dbContext.Users.AddAsync(newUser);
            await dbContext.SaveChangesAsync();
            var userResponse =
                $"Пользователь {newUser.Handle ?? newUser.Name} ({newUser.Id}) успешно добавлен в базу данных.";
            return UserResponseFactory.Success(userResponse);
        }
        catch (ApiRequestException)
        {
            return UserResponseFactory.FailedSendingApiRequest(Name);
        }
    }
}