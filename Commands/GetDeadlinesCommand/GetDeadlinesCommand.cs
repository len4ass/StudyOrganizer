using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Models.Deadline;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Deadline;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace GetDeadlinesCommand;

public class GetDeadlinesCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly GeneralSettings _settings;

    public GetDeadlinesCommand(PooledDbContextFactory<MyDbContext> dbContextFactory, GeneralSettings settings)
    {
        Name = "getdeadlines";
        Description = "Получает все актуальные дедлайны из базы данных.";
        Format = "/getdeadlines";
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
        if (arguments.Count > 0)
        {
            return BotResponseFactory.ArgumentLimitExceeded(
                Name,
                userInfo.Handle!,
                userInfo.Id);
        }

        var validDeadlines = await ExtractValidDeadlines();
        var sortedDeadlines = validDeadlines.OrderBy(deadline => deadline.DateUtc);
        var deadlinesString = BuildDeadlineString(sortedDeadlines);
        return BotResponseFactory.Success(
            Name, 
            deadlinesString, 
            userInfo.Handle!, 
            userInfo.Id);
    }

    private async Task<IEnumerable<DeadlineInfo>> ExtractValidDeadlines()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deadlineRepository = new DeadlineInfoRepository(dbContext);
        
        var allDeadlines = await deadlineRepository.GetDataAsync();
        return allDeadlines.Where(deadline => deadline.DateUtc > DateTimeOffset.UtcNow).ToList();
    }

    private string GetDeadlineString(DeadlineInfo deadline, int index)
    {
        var (parsed, timeZoneInfo) = TimeZoneInfoExtensions.TryParse(_settings.ChatTimeZoneUtc);
        if (!parsed)
        {
            return string.Empty;
        }

        return $"<b>{index}</b>. {deadline.ToString(timeZoneInfo!)}";
    }
    
    private string BuildDeadlineString(IOrderedEnumerable<DeadlineInfo> deadlines)
    {
        int index = 1;
        var sb = new StringBuilder("Список текущих дедлайнов: \n\n");
        foreach (var deadline in deadlines)
        {
            string deadlineString = GetDeadlineString(deadline, index);
            if (deadlineString != string.Empty)
            {
                sb.AppendLine(deadlineString);
                index++;
            }
        }

        if (index == 1)
        {
            return "Дедлайнов нет :)";
        }

        return sb.ToString();
    }
}