using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Helper;
using StudyOrganizer.Models.Deadline;
using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Repositories.Deadline;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace AddDeadlineCommand;

public class AddDeadlineCommand : BotCommand
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
    
    public override async Task<BotResponse> ExecuteAsync(
        ITelegramBotClient client,
        Message message, 
        UserInfo userInfo, 
        IList<string> arguments)
    {
        var response = await ParseResponse(userInfo, arguments);
        await BotMessager.Reply(
            client,
            message,
            response.UserResponse);

        return response;
    }

    private async Task<BotResponse> ParseResponse(UserInfo userInfo, IList<string> arguments)
    {
        if (arguments.Count < 2)
        {
            return BotResponseFactory.NotEnoughArguments(
                Name, 
                userInfo.Handle!, 
                userInfo.Id);
        }

        string fullCommand = string.Join(' ', arguments);
        var match = new Regex(RegexHelper.DateTimeRegex).Match(fullCommand);
        if (!match.Success)
        {
            return BotResponseFactory.FailedParsing(
                Name, 
                userInfo.Handle!, 
                userInfo.Id);
        }

        string name = arguments[0];
        string dateTimeString = match.Value;
        string description = string.Join(' ', fullCommand
            .Replace(name, "")
            .Replace(dateTimeString, "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)).Trim();

        if (description.Length == 0)
        {
            description = "Нет описания";
        }

        var dateTimeOffset = TextParser.ParseDateTime(dateTimeString, _settings.ChatTimeZoneUtc);
        if (dateTimeOffset is null || dateTimeOffset < DateTimeOffset.UtcNow)
        {
            return BotResponseFactory.FailedParsing(Name, userInfo.Handle!, userInfo.Id);
        }

        return await AddDeadlineToDatabase(
            userInfo, 
            name, 
            dateTimeOffset.Value, 
            description);
    }

    private async Task<BotResponse> AddDeadlineToDatabase(
        UserInfo userInfo,
        string name,
        DateTimeOffset dateTimeUtc,
        string description)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deadlineRepository = new DeadlineInfoRepository(dbContext);

        var deadlineInDatabase = await deadlineRepository.FindAsync(name);
        if (deadlineInDatabase is not null)
        {
            return BotResponseFactory.EntryAlreadyExists(
                Name, 
                "дедлайн", 
                name, 
                userInfo.Handle!,
                userInfo.Id);
        }
        
        var deadline = new DeadlineInfo(name, description, dateTimeUtc);
        await deadlineRepository.AddAsync(deadline);
        await deadlineRepository.SaveAsync();
        return BotResponseFactory.Success(
            Name, 
            $"Дедлайн <b>{name}</b> с датой {dateTimeUtc.UtcDateTime:dd.MM.yyyy HH:mm:ss} (UTC) успешно добавлен в базу данных.", 
            userInfo.Handle!,
            userInfo.Id);
    }
}