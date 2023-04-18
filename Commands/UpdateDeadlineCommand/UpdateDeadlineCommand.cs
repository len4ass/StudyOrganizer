using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Helper;
using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace UpdateDeadlineCommand;

public sealed class UpdateDeadlineCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly GeneralSettings _settings;

    public UpdateDeadlineCommand(PooledDbContextFactory<MyDbContext> dbContextFactory, GeneralSettings settings)
    {
        Name = "updatedeadline";
        Description = "Обновляет информацию о дедлайне в базе данных.";
        Format = "/updatedeadline <name> <DateTime> <optional:description>";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Normal
        };

        _dbContextFactory = dbContextFactory;
        _settings = settings;
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
        if (arguments.Count < 2)
        {
            return UserResponseFactory.NotEnoughArguments(Name);
        }

        var fullCommand = string.Join(' ', arguments);
        var match = new Regex(RegexHelper.DateTimeRegex).Match(fullCommand);
        if (!match.Success)
        {
            return UserResponseFactory.FailedParsingSpecified(
                Name,
                "в вашем вводе не содержится дата подходящего формата");
        }

        var name = arguments[0];
        var dateTimeString = match.Value;
        var description = string.Join(
                ' ',
                fullCommand
                    .Replace(name, "")
                    .Replace(dateTimeString, "")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Trim();

        if (description.Length == 0)
        {
            description = "Нет описания";
        }

        var dateTimeOffset = TextParser.ParseDateTime(dateTimeString, _settings.ChatTimeZoneUtc);
        if (dateTimeOffset is null || dateTimeOffset < DateTimeOffset.UtcNow)
        {
            return UserResponseFactory.FailedParsingSpecified(Name, "указанная вами дата некорректна.");
        }

        return await UpdateDeadlineInDatabase(
            name,
            dateTimeOffset.Value,
            description);
    }

    private async Task<UserResponse> UpdateDeadlineInDatabase(
        string name,
        DateTimeOffset newDateTimeUtc,
        string newDescription)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deadlineInDatabase = await dbContext.Deadlines.FindAsync(name);
        if (deadlineInDatabase is null)
        {
            return UserResponseFactory.EntryDoesNotExist(
                Name,
                "дедлайн",
                name);
        }

        var previousDescription = deadlineInDatabase.Description;
        var previousDateUtc = deadlineInDatabase.DateUtc;

        deadlineInDatabase.Description = newDescription;
        deadlineInDatabase.DateUtc = newDateTimeUtc;
        await dbContext.SaveChangesAsync();

        return UserResponseFactory.Success(
            $"Дедлайн <b>{name}</b> изменен: \n" +
            $"Дата изменена с {previousDateUtc.UtcDateTime:dd.MM.yyyy HH:mm:ss} (UTC) на {newDateTimeUtc.UtcDateTime:dd.MM.yyyy HH:mm:ss} (UTC).\n" +
            $"Описание изменено с '{previousDescription}' на '{newDescription}'");
    }
}