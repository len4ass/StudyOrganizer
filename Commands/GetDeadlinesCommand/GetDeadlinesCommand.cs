using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.Deadline;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Deadline;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace GetDeadlinesCommand;

public sealed class GetDeadlinesCommand : BotCommand
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

        var validDeadlines = await ExtractValidDeadlines();
        var sortedDeadlines = validDeadlines.OrderBy(deadline => deadline.DateUtc);
        var deadlinesString = BuildDeadlineString(sortedDeadlines);
        return UserResponseFactory.Success(deadlinesString);
    }

    private async Task<IEnumerable<DeadlineInfo>> ExtractValidDeadlines()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deadlineRepository = new DeadlineInfoRepository(dbContext);

        var allDeadlines = await deadlineRepository.GetDataAsync();
        return allDeadlines.Where(deadline => deadline.DateUtc > DateTimeOffset.UtcNow)
            .ToList();
    }

    private string GetDeadlineString(DeadlineInfo deadline, int index)
    {
        return $"<b>{index}</b>. {deadline.ToString(_settings.ChatTimeZoneUtc)}";
    }

    private string BuildDeadlineString(IOrderedEnumerable<DeadlineInfo> deadlines)
    {
        var index = 1;
        var sb = new StringBuilder("Список текущих дедлайнов: \n\n");
        foreach (var deadline in deadlines)
        {
            var deadlineString = GetDeadlineString(deadline, index);
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