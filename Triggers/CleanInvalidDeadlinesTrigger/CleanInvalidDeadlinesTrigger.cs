using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Quartz;
using StudyOrganizer.Database;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings;
using StudyOrganizer.Settings.SimpleTrigger;
using Telegram.Bot;

namespace CleanInvalidDeadlinesTrigger;

public sealed class CleanInvalidDeadlinesTrigger : SimpleTrigger
{
    private readonly ITelegramBotClient _client;
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly GeneralSettings _settings;

    public CleanInvalidDeadlinesTrigger(
        ITelegramBotClient client,
        PooledDbContextFactory<MyDbContext> dbContextFactory,
        GeneralSettings settings)
    {
        Name = "deadline_cleaner";
        Description = "Удаляет истекшие дедлайны каждый день.";
        Settings = new TriggerSettings
        {
            ShouldRun = true,
            HourUtc = 01,
            MinuteUtc = 00,
            SecondUtc = 00,
            RecurringType = SimpleTriggerRecurringType.Daily,
            DayOfWeek = SimpleTriggerDayOfWeek.MON
        };

        _client = client;
        _dbContextFactory = dbContextFactory;
        _settings = settings;
    }

    public override async Task Execute(IJobExecutionContext context)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deadlines = await dbContext.Deadlines.ToListAsync();

        var expiredDeadlines = deadlines.Where(deadline => deadline.DateUtc < DateTimeOffset.UtcNow);
        dbContext.RemoveRange(expiredDeadlines);
        await dbContext.SaveChangesAsync();
    }
}