using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings;
using Telegram.Bot;

namespace HelloSimpleTrigger;

public class HelloSimpleTrigger : SimpleTrigger
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
            HourUtc = 09,
            MinuteUtc = 32,
            SecondUtc = 0,
            RunEveryGivenSeconds = 30
        };
    }

    public override async Task ExecuteAsync()
    {
        await BotMessager.Send(_client, _settings.MainChatId, "hello from cronjob");
    }
}