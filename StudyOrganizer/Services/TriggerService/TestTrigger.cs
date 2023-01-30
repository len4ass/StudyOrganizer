using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;

namespace StudyOrganizer.Services.TriggerService;

public class TestTrigger : CronTrigger
{
    public TestTrigger(
        IMasterRepository masterRepository, 
        ITelegramBotClient client,
        GeneralSettings generalSettings) 
        : base(masterRepository, client, generalSettings)
    {
        Hour = 14;
        Minute = 16;
        Second = 0;
        RunEveryGivenSeconds = 86400; // каждый день
    }

    public override async Task ExecuteAsync()
    {
        await BotMessager.Send(Client, GeneralSettings.MainChatId, "hello from cronjob");
    }
}