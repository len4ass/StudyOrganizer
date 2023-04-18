using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Quartz;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings;
using StudyOrganizer.Settings.SimpleTrigger;
using Telegram.Bot;

namespace BirthdayAnnouncementTrigger;

public class BirthdayAnnouncementTrigger : SimpleTrigger
{
    private readonly ITelegramBotClient _client;
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly GeneralSettings _settings;

    public BirthdayAnnouncementTrigger(
        ITelegramBotClient client,
        PooledDbContextFactory<MyDbContext> dbContextFactory,
        GeneralSettings settings)
    {
        Name = "birthday_announcement";
        Description = "Оповещает о днях рождения пользователей.";
        Settings = new TriggerSettings
        {
            ShouldRun = true,
            HourUtc = 21,
            MinuteUtc = 00,
            SecondUtc = 00,
            RecurringType = SimpleTriggerRecurringType.Daily,
            DayOfWeek = SimpleTriggerDayOfWeek.MON
        };

        _client = client;
        _dbContextFactory = dbContextFactory;
        _settings = settings;
    }

    private string GetBirthdayString(UserInfo userInfo)
    {
        return $"<b>{userInfo.GetCorrectTagFormatting()}</b>" +
               "\nС ДНЕМ РОЖДЕНИЯ!!! УСПЕХОВ РАДОСТИ ВЕЗЕНИЯ!!! И ЧУМОВОГО НАСТРОЕНИЯ!!!" +
               "\nПОЗДРАВЛЯЕМ! 🥳";
    }

    public override async Task Execute(IJobExecutionContext context)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var usersWithBirthday = await dbContext.Users
            .Where(user => user.Birthday.HasValue)
            .ToListAsync();
        var usersWithBirthdayToday = usersWithBirthday
            .Where(
                user => DateTimeOffset.UtcNow.DayOfYear ==
                        user.GetBirthdayUtc(_settings.ChatTimeZoneUtc)
                            .DayOfYear)
            .ToList();

        foreach (var user in usersWithBirthdayToday)
        {
            await BotMessager.Send(
                _client,
                _settings.ImportantChatId,
                GetBirthdayString(user));
        }
    }
}