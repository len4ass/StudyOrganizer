using StudyOrganizer.Enum;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Services.BotService;

public abstract class BotCommand
{
    public string Name { get; init; }
    public string Description { get; init; }
    public AccessLevel AccessLevel { get; init; }
    
    protected readonly IMasterRepository MasterRepository;
    protected readonly GeneralSettings GeneralSettings;

    protected BotCommand(IMasterRepository masterRepository, GeneralSettings generalSettings)
    {
        Name = "command";
        Description = "No description";
        AccessLevel = AccessLevel.Normal;
        MasterRepository = masterRepository;
        GeneralSettings = generalSettings;
    }

    public abstract Task<string> ExecuteAsync(
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments);
}