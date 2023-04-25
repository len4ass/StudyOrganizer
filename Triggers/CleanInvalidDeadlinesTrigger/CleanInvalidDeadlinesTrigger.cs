using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Quartz;
using StudyOrganizer.Database;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings.SimpleTrigger;

namespace CleanInvalidDeadlinesTrigger;

public sealed class CleanInvalidDeadlinesTrigger : SimpleTrigger
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public CleanInvalidDeadlinesTrigger(PooledDbContextFactory<MyDbContext> dbContextFactory)
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

        _dbContextFactory = dbContextFactory;
    }

    public override async Task Execute(IJobExecutionContext context)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            @"DELETE FROM Deadlines 
              WHERE (strftime('%s', DateUtc) < strftime('%s', 'now'))");
    }
}