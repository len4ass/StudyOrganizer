using System.ComponentModel.DataAnnotations;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Services.BotService.Command;

public abstract class BotCommand
{
    public string Name { get; init; }
    public string Description { get; init; }
    public AccessLevel AccessLevel { get; set; }
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

    public abstract Task<BotResponse> ExecuteAsync(
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments);


    public override string ToString()
    {
        return $"/{Name}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BotCommand command)
        {
            return false;
        }
        
        return Equals(command);
    }

    private bool Equals(BotCommand other)
    {
        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Name.GetHashCode() * 397) ^ Description.GetHashCode();
        }
    }
}