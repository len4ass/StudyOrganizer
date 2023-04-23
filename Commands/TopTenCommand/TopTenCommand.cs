using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace TopTenCommand;

public sealed class TopTenCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly GeneralSettings _generalSettings;

    public TopTenCommand(PooledDbContextFactory<MyDbContext> dbContextFactory, GeneralSettings generalSettings)
    {
        Name = "top10";
        Description = "Выводит список топ10 пользователей по указанному параметру.";
        Format =
            $"/top10 <{EnumExtensions.BuildOptionalSlashStringFromEnum<TopParameterType>()}> <optional:isReversed>";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Normal
        };

        _dbContextFactory = dbContextFactory;
        _generalSettings = generalSettings;
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

        var parsedParameterType = Enum.TryParse<TopParameterType>(arguments[0], out var topParameterType);
        if (!parsedParameterType)
        {
            return UserResponseFactory.FailedParsing(Name);
        }

        return arguments.Count switch
        {
            1 => await FormatFirstTenUsersByParameterType(topParameterType),
            2 => await FormatFirstTenUsersByParameterType(topParameterType, true),
            _ => UserResponseFactory.ArgumentLimitExceeded(Name)
        };
    }

    private async Task<UserResponse> FormatFirstTenUsersByParameterType(TopParameterType type, bool reversed = false)
    {
        var userResponse = type switch
        {
            TopParameterType.Coolest => await FormatTopTenCoolestUsers(reversed),
            TopParameterType.MessageCount => await FormatTopTenUsersByMessageCount(reversed),
            TopParameterType.Age => await FormatTopTenUsersByAge(reversed),
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                null)
        };

        return UserResponseFactory.Success(userResponse);
    }

    private async Task<string> FormatTopTenCoolestUsers(bool reversed = false)
    {
        var users = await GetTopTenCoolestUsers(reversed);
        if (users.Count == 0)
        {
            return "Нет доступных пользователей для выполнения команды.";
        }

        var userCount = users.Count < 10 ? users.Count : 10;
        var optionalString = reversed ? "Анти" : string.Empty;
        var sb = new StringBuilder($"{optionalString}Топ-{userCount} красавчиков: \n\n");
        for (var i = 0; i < users.Count; i++)
        {
            sb.AppendLine($"<b>{i + 1}</b>. {users[i].Handle ?? users[i].Name} — <em>{users[i].WonCOTD} раз(а)</em>");
        }

        return sb.ToString();
    }

    private async Task<string> FormatTopTenUsersByMessageCount(bool reversed = false)
    {
        var users = await GetTopTenUsersByMessageCount(reversed);
        if (users.Count == 0)
        {
            return "Нет доступных пользователей для выполнения команды.";
        }

        var userCount = users.Count < 10 ? users.Count : 10;
        var optionalString = reversed ? "Анти" : string.Empty;
        var sb = new StringBuilder($"{optionalString}Топ-{userCount} по количеству сообщений: \n\n");
        for (var i = 0; i < users.Count; i++)
        {
            sb.AppendLine(
                $"<b>{i + 1}</b>. {users[i].Handle ?? users[i].Name} — <em>{users[i].MsgAmount} сообщений</em>");
        }

        return sb.ToString();
    }

    private async Task<string> FormatTopTenUsersByAge(bool reversed = false)
    {
        var users = await GetTopTenUsersByAge(reversed);
        if (users.Count == 0)
        {
            return "Нет доступных пользователей для выполнения команды.";
        }

        var userCount = users.Count < 10 ? users.Count : 10;
        var optionalString = reversed ? "Анти" : string.Empty;
        var sb = new StringBuilder($"{optionalString}Топ-{userCount} по возрасту: \n\n");
        for (var i = 0; i < users.Count; i++)
        {
            sb.AppendLine(
                $"<b>{i + 1}</b>. {users[i].Handle ?? users[i].Name} — <em>{users[i].GetAge(_generalSettings.ChatTimeZoneUtc)} лет</em>");
        }

        return sb.ToString();
    }


    private async Task<IList<UserInfo>> GetTopTenCoolestUsers(bool reversed = false)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (reversed)
        {
            return await dbContext.Users.OrderBy(user => user.WonCOTD)
                .Take(10)
                .Reverse()
                .ToListAsync();
        }

        return await dbContext.Users.OrderByDescending(user => user.WonCOTD)
            .Take(10)
            .ToListAsync();
    }

    private async Task<IList<UserInfo>> GetTopTenUsersByMessageCount(bool reversed = false)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (reversed)
        {
            return await dbContext.Users.OrderBy(user => user.MsgAmount)
                .Take(10)
                .Reverse()
                .ToListAsync();
        }

        return await dbContext.Users.OrderByDescending(user => user.MsgAmount)
            .Take(10)
            .ToListAsync();
    }

    private async Task<IList<UserInfo>> GetTopTenUsersByAge(bool reversed = false)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (reversed)
        {
            return await dbContext.Users.Where(user => user.Birthday.HasValue)
                .OrderByDescending(user => user.Birthday!.Value)
                .Take(10)
                .Reverse()
                .ToListAsync();
        }

        return await dbContext.Users.Where(user => user.Birthday.HasValue)
            .OrderBy(user => user.Birthday!.Value)
            .Take(10)
            .ToListAsync();
    }
}