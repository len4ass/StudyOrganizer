using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings;
using Telegram.Bot;

namespace HelloSimpleTrigger;

public class HelloSimpleTrigger : SimpleTrigger
{
    public HelloSimpleTrigger(
        IMasterRepository masterRepository, 
        ITelegramBotClient client, 
        GeneralSettings generalSettings) 
        : base(masterRepository, client, generalSettings)
    {
        Settings = new TriggerSettings
        {
            ShouldRun = true,
            Hour = 22,
            Minute = 17,
            Second = 0,
            RunEveryGivenSeconds = 30
        };
    }

    public override async Task ExecuteAsync()
    {
        await BotMessager.Send(Client, GeneralSettings.MainChatId, "hello from cronjob");
    }
}