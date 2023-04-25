using System.Text;
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

namespace GetBirthdaysCommand;

public sealed class GetBirthdaysCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public GetBirthdaysCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "getbirthdays";
        Description = "Выводит список дней рождений пользователей.";
        Format = "/getbirthdays";
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
        if (arguments.Count > 0)
        {
            return UserResponseFactory.ArgumentLimitExceeded(Name);
        }

        return await GetAllBirthdays();
    }

    private string FormatAllUserBirthdays(IList<UserInfo> users)
    {
        var sb = new StringBuilder("Все дни рождения пользователей: \n\n");
        for (var i = 0; i < users.Count; i++)
        {
            sb.AppendLine($"<b>{i + 1}</b>. {users[i].Handle ?? users[i].Name} — {users[i].GetBirthdayString()}");
        }

        return sb.ToString();
    }

    private async Task<UserResponse> GetAllBirthdays()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var usersWithDefinedBirthday = await dbContext.Users
            .Where(user => user.Birthday.HasValue)
            .OrderBy(user => user.Birthday!.Value)
            .ToListAsync();

        var userBirthdaysFormatted = FormatAllUserBirthdays(usersWithDefinedBirthday);
        return new UserResponse(userBirthdaysFormatted);
    }
}