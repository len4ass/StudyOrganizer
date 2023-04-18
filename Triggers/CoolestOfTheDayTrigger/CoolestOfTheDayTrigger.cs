using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Quartz;
using StudyOrganizer.Database;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings;
using StudyOrganizer.Settings.SimpleTrigger;
using Telegram.Bot;

namespace CoolestOfTheDayTrigger;

public sealed class CoolestOfTheDayTrigger : SimpleTrigger
{
    private readonly ITelegramBotClient _client;
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly GeneralSettings _settings;

    public CoolestOfTheDayTrigger(
        ITelegramBotClient client,
        PooledDbContextFactory<MyDbContext> dbContextFactory,
        GeneralSettings settings)
    {
        Name = "coolest_of_the_day";
        Description = "Определяет красавчика дня.";
        _client = client;
        _dbContextFactory = dbContextFactory;
        _settings = settings;

        Settings = new TriggerSettings
        {
            ShouldRun = true,
            HourUtc = 12,
            MinuteUtc = 00,
            SecondUtc = 00,
            RecurringType = SimpleTriggerRecurringType.Daily,
            DayOfWeek = SimpleTriggerDayOfWeek.MON
        };
    }

    public override async Task Execute(IJobExecutionContext context)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var previousUserCoolest = await dbContext.Users.FirstOrDefaultAsync(user => user.CoolestOfTheDay);
        if (previousUserCoolest is not null)
        {
            previousUserCoolest.CoolestOfTheDay = false;
        }

        var skipCount = Random.Shared.Next(0, dbContext.Users.Count());
        var userCoolest = await dbContext.Users.Skip(skipCount)
            .FirstOrDefaultAsync();
        if (userCoolest is null)
        {
            return;
        }

        userCoolest.CoolestOfTheDay = true;
        userCoolest.WonCOTD++;
        await dbContext.SaveChangesAsync();

        var message = "<b>*Барабанная дробь*</b>" +
                      $"\n<em>Красавчиком дня становится...</em> {userCoolest.GetCorrectTagFormatting()}" +
                      "\nПоздравляем! 🥳";
        await BotMessager.Send(
            _client,
            _settings.MainChatId,
            message);
    }
}