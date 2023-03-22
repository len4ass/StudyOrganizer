using System.ComponentModel.DataAnnotations;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Services.TriggerService;

public abstract class SimpleTrigger
{
    public string Name { get; init; }
    public string Description { get; init; }

    public TriggerSettings Settings { get; set; }
    
    protected readonly IMasterRepository MasterRepository;
    protected readonly GeneralSettings GeneralSettings;
    protected readonly ITelegramBotClient Client;

    protected SimpleTrigger(
        IMasterRepository masterRepository, 
        ITelegramBotClient client, 
        GeneralSettings generalSettings)
    {
        Name = "trigger";
        Description = "No description";
        Settings = new TriggerSettings();
        MasterRepository = masterRepository;
        Client = client;
        GeneralSettings = generalSettings;
    }

    public abstract Task ExecuteAsync();
}