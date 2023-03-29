using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Helper;
using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Repositories.Deadline;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace UpdateDeadlineCommand;

public class UpdateDeadlineCommand : BotCommand
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

        var name = arguments[0];
        var dateTimeString = match.Value;
        var description = string.Join(' ', fullCommand
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

        return await UpdateDeadlineInDatabase(
            userInfo, 
            name, 
            dateTimeOffset.Value, 
            description);
    }

    private async Task<BotResponse> UpdateDeadlineInDatabase(
        UserInfo userInfo,
        string name,
        DateTimeOffset newDateTimeUtc,
        string newDescription)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deadlineRepository = new DeadlineInfoRepository(dbContext);

        var deadlineInDatabase = await deadlineRepository.FindAsync(name);
        if (deadlineInDatabase is null)
        {
            return BotResponseFactory.EntryDoesNotExist(
                Name, 
                "дедлайн", 
                name, 
                userInfo.Handle!,
                userInfo.Id);
        }

        var previousDescription = deadlineInDatabase.Description;
        var previousDateUtc = deadlineInDatabase.DateUtc;
        
        deadlineInDatabase.Description = newDescription;
        deadlineInDatabase.DateUtc = newDateTimeUtc;
        await deadlineRepository.SaveAsync();

        return BotResponseFactory.Success(
            Name, 
            $"Дедлайн <b>{name}</b> изменен: \n" +
            $"Дата изменена с {previousDateUtc.UtcDateTime:dd.MM.yyyy HH:mm:ss} (UTC) на {newDateTimeUtc.UtcDateTime:dd.MM.yyyy HH:mm:ss} (UTC).\n" +
            $"Описание изменено с '{previousDescription}' на '{newDescription}'", 
            userInfo.Handle!,
            userInfo.Id);
    }
}