using Quartz;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings;
using StudyOrganizer.Settings.SimpleTrigger;
using Telegram.Bot;

namespace HelloSimpleTrigger;

public sealed class HelloSimpleTrigger : SimpleTrigger
{
    private readonly ITelegramBotClient _client;
    private readonly GeneralSettings _settings;

    public HelloSimpleTrigger(ITelegramBotClient client, GeneralSettings generalSettings)
    {
        _client = client;
        _settings = generalSettings;
        Settings = new TriggerSettings
        {
            ShouldRun = true,
            HourUtc = 07,
            MinuteUtc = 05,
            SecondUtc = 0,
            RecurringType = SimpleTriggerRecurringType.EveryMinute,
            DayOfWeek = SimpleTriggerDayOfWeek.MON
        };
    }

    public override async Task Execute(IJobExecutionContext context)
    {
        await BotMessager.Send(
            _client,
            _settings.MainChatId,
            "hello from cronjob");
    }
}