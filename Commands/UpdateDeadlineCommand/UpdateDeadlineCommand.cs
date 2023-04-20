using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Helper;
using StudyOrganizer.Models.Deadline;
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
    private readonly IValidator<DeadlineInfo> _deadlineValidator;
    private readonly GeneralSettings _settings;

    public UpdateDeadlineCommand(
        PooledDbContextFactory<MyDbContext> dbContextFactory,
        IValidator<DeadlineInfo> deadlineValidator,
        GeneralSettings settings)
    {
        Name = "updatedeadline";
        Description = "Обновляет информацию о дедлайне в базе данных.";
        Format = "/updatedeadline <name> <DateTime> <optional:description>";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Normal
        };

        _dbContextFactory = dbContextFactory;
        _deadlineValidator = deadlineValidator;
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
            return UserResponseFactory.FailedParsingSpecified(Name, "Не найдено корректной даты.");
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
        if (dateTimeOffset is null)
        {
            return UserResponseFactory.FailedParsingSpecified(Name, "Не удалось конвертировать дату.");
        }

        var deadlineInfo = new DeadlineInfo(
            name,
            description,
            dateTimeOffset.Value);
        var (isValid, response) = ValidateDeadline(deadlineInfo);
        if (!isValid)
        {
            return response;
        }

        return await UpdateDeadlineInDatabase(deadlineInfo);
    }

    private (bool, UserResponse) ValidateDeadline(DeadlineInfo deadlineInfo)
    {
        var result = _deadlineValidator.Validate(deadlineInfo);
        if (!result.IsValid)
        {
            return (false, UserResponseFactory.FailedParsingSpecified(Name, result.ToString()));
        }

        return (true, default!);
    }

    private async Task<UserResponse> UpdateDeadlineInDatabase(DeadlineInfo deadlineInfo)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deadlineInDatabase = await dbContext.Deadlines.FindAsync(deadlineInfo.Name);
        if (deadlineInDatabase is null)
        {
            return UserResponseFactory.DeadlineDoesNotExist(Name, deadlineInfo.Name);
        }

        var previousDescription = deadlineInDatabase.Description;
        var previousDateUtc = deadlineInDatabase.DateUtc;

        deadlineInDatabase.Description = deadlineInfo.Description;
        deadlineInDatabase.DateUtc = deadlineInfo.DateUtc;
        await dbContext.SaveChangesAsync();

        return UserResponseFactory.Success(
            $"Дедлайн <b>{deadlineInDatabase.Name}</b> изменен: \n" +
            $"Дата изменена с {previousDateUtc.UtcDateTime:dd.MM.yyyy HH:mm:ss} (UTC) на " +
            $"{deadlineInfo.DateUtc:dd.MM.yyyy HH:mm:ss} (UTC).\n" +
            $"Описание изменено с '{previousDescription}' на '{deadlineInfo.Description}'.");
    }
}