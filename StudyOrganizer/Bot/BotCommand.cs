using StudyOrganizer.Enum;
using StudyOrganizer.Models;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Bot;

public abstract class BotCommand
{
    public string Name { get; init; }
    public string Description { get; init; }
    public AccessLevel AccessLevel { get; init; }
    
    protected readonly IRepository MasterRepository;
    protected readonly Settings.GeneralSettings GeneralSettings;

    protected BotCommand(IRepository masterRepository, Settings.GeneralSettings generalSettings)
    {
        Name = "command";
        Description = "No description";
        AccessLevel = AccessLevel.Normal;
        MasterRepository = masterRepository;
        GeneralSettings = generalSettings;
    }

    public abstract Task<string> Execute(
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments);
}