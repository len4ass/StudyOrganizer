using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Quartz;
using StudyOrganizer.Database;
using StudyOrganizer.Models.Deadline;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings;
using StudyOrganizer.Settings.SimpleTrigger;
using Telegram.Bot;

namespace DeadlineAnnouncementTrigger;

public sealed class DeadlineAnnouncementTrigger : SimpleTrigger
{
    private readonly ITelegramBotClient _client;
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly GeneralSettings _settings;

    public DeadlineAnnouncementTrigger(
        ITelegramBotClient client,
        PooledDbContextFactory<MyDbContext> dbContextFactory,
        GeneralSettings settings)
    {
        Name = "deadline_announcement";
        Description = "Оповещает о дедлайнах каждый день.";
        Settings = new TriggerSettings
        {
            ShouldRun = true,
            HourUtc = 05,
            MinuteUtc = 00,
            SecondUtc = 00,
            RecurringType = SimpleTriggerRecurringType.Daily,
            DayOfWeek = SimpleTriggerDayOfWeek.MON
        };

        _client = client;
        _dbContextFactory = dbContextFactory;
        _settings = settings;
    }

    private string GetDeadlineStringShortened(DeadlineInfo deadline, int index)
    {
        return $"<b>{index}</b>. {deadline.ToStringShortened(_settings.ChatTimeZoneUtc)}";
    }

    private string BuildListOfDeadlinesString(IList<DeadlineInfo> deadlines)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < deadlines.Count; i++)
        {
            sb.AppendLine(GetDeadlineStringShortened(deadlines[i], i + 1));
        }

        return sb.ToString();
    }

    private string BuildDeadlineAnnouncementString(
        IList<DeadlineInfo> deadlinesToday,
        IList<DeadlineInfo> deadlinesDuringTheWeek)
    {
        if (deadlinesToday.Count == 0 && deadlinesDuringTheWeek.Count == 0)
        {
            return "Ни сегодня, ни в течении недели нет никаких дедлайнов :)";
        }

        if (deadlinesToday.Count == 0 && deadlinesDuringTheWeek.Count != 0)
        {
            var deadlines = BuildListOfDeadlinesString(deadlinesDuringTheWeek);
            return $"Дедлайнов сегодня нет, а вот в течении недели есть: \n\n{deadlines}";
        }

        if (deadlinesToday.Count != 0 && deadlinesDuringTheWeek.Count != 0)
        {
            var deadlines = BuildListOfDeadlinesString(deadlinesToday);
            return $"В течении недели дедлайнов нет. А вот сегодня есть: \n\n{deadlines}";
        }

        var today = BuildListOfDeadlinesString(deadlinesToday);
        var duringTheWeek = BuildListOfDeadlinesString(deadlinesDuringTheWeek);
        return $"Дедлайны сегодня: \n\n{today} " +
               $"\nДедлайны в ближайшие семь дней: \n\n{duringTheWeek}";
    }


    public override async Task Execute(IJobExecutionContext context)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deadlines = await dbContext.Deadlines.ToListAsync();

        var deadlinesForToday = deadlines
            .Where(deadline => deadline.IsInGivenDayRange(1))
            .ToList();
        var deadlinesForTheRestOfTheWeek = deadlines
            .Where(deadline => deadline.IsInGivenDayRange(1, 7))
            .ToList();

        var deadlineString = BuildDeadlineAnnouncementString(deadlinesForToday, deadlinesForTheRestOfTheWeek);
        await BotMessager.Send(
            _client,
            _settings.ImportantChatId,
            deadlineString);
    }
}