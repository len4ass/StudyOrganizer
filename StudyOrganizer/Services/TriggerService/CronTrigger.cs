using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Services.TriggerService;

public abstract class CronTrigger
{
    public string Name { get; init; }
    public string Description { get; init; }
    
    public int Hour { get; init; }
    public int Minute { get; init; }
    public int Second { get; init; }
    public int RunEveryGivenSeconds { get; init; }

    protected readonly IMasterRepository MasterRepository;
    protected readonly GeneralSettings GeneralSettings;
    protected readonly ITelegramBotClient Client;

    protected CronTrigger(IMasterRepository masterRepository, 
        ITelegramBotClient client, GeneralSettings generalSettings)
    {
        Name = "trigger";
        Description = "No description";
        MasterRepository = masterRepository;
        Client = client;
        GeneralSettings = generalSettings;
    }

    public abstract Task ExecuteAsync();
}