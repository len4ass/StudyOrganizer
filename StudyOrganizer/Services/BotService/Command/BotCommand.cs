using StudyOrganizer.Models.User;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Services.BotService.Command;

public abstract class BotCommand
{
    public string Name { get; init; }
    public string Description { get; init; }
    
    public string Format { get; init; }
    
    public CommandSettings Settings { get; set; }

    protected BotCommand()
    {
        Name = "command";
        Description = "No description";
        Format = "/command";
        Settings = new CommandSettings();
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