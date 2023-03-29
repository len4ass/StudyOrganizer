using Microsoft.EntityFrameworkCore.Infrastructure;
using Quartz;
using StudyOrganizer.Database;
using StudyOrganizer.Repositories.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings;
using Telegram.Bot;

namespace CoolestOfTheDayTrigger;

public class CoolestOfTheDayTrigger : SimpleTrigger
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
        Description = "Определяет красавчика дня";
        _client = client;
        _dbContextFactory = dbContextFactory;
        _settings = settings;

        Settings = new TriggerSettings
        {
            ShouldRun = true,
            HourUtc = 12,
            MinuteUtc = 00,
            SecondUtc = 00,
            RecurringType = SimpleTriggerRecurringType.Daily
        };
    }

    public override async Task Execute(IJobExecutionContext context)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var userInfoRepository = new UserInfoRepository(dbContext);
        var previousUserCoolest = await userInfoRepository.FindByPredicateAsync(user => user.CoolestOfTheDay);
        if (previousUserCoolest is not null)
        {
            previousUserCoolest.CoolestOfTheDay = false;
        }

        var skipCount = Random.Shared.Next(0, userInfoRepository.Count());
        var userCoolest = await userInfoRepository.SkipAndTakeFirst(skipCount);
        userCoolest.CoolestOfTheDay = true;
        userCoolest.WonCOTD++;

        await userInfoRepository.SaveAsync();
        
        var message = "<b>*Барабанная дробь*</b>" +
                      $"\n<em>Красавчиком дня становится...</em> @{userCoolest.Handle}" +
                      "\nПоздравляем! 🥳";
        await BotMessager.Send(_client, _settings.MainChatId, message);
    }
}