using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Helper;
using StudyOrganizer.Models.Deadline;
using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Repositories.Deadline;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace AddDeadlineCommand;

public sealed class AddDeadlineCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly GeneralSettings _settings;

    public AddDeadlineCommand(PooledDbContextFactory<MyDbContext> dbContextFactory, GeneralSettings settings)
    {
        Name = "adddeadline";
        Description = "Добавляет дедлайн в базу данных по имени, времени и описанию.";
        Format = "/adddeadline <name> <DateTime> <optional:description>";
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
            return UserResponseFactory.FailedParsing(Name);
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
            return UserResponseFactory.FailedParsing(Name);
        }

        return await AddDeadlineToDatabase(
            name,
            dateTimeOffset.Value,
            description);
    }

    private async Task<UserResponse> AddDeadlineToDatabase(
        string name,
        DateTimeOffset dateTimeUtc,
        string description)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deadlineRepository = new DeadlineInfoRepository(dbContext);

        var deadlineInDatabase = await deadlineRepository.FindAsync(name);
        if (deadlineInDatabase is not null)
        {
            return UserResponseFactory.EntryAlreadyExists(
                Name,
                "дедлайн",
                name);
        }

        var deadline = new DeadlineInfo(
            name,
            description,
            dateTimeUtc);

        await deadlineRepository.AddAsync(deadline);
        await deadlineRepository.SaveAsync();

        var response =
            $"Дедлайн <b>{name}</b> с датой {dateTimeUtc.UtcDateTime:dd.MM.yyyy HH:mm:ss} (UTC) успешно добавлен в базу данных.";
        return UserResponseFactory.Success(response);
    }
}