using StudyOrganizer.Settings;

namespace StudyOrganizer.Services.TriggerService;

public abstract class SimpleTrigger
{
    public string Name { get; init; }
    public string Description { get; init; }

    public TriggerSettings Settings { get; set; }
    
    protected SimpleTrigger()
    {
        Name = "trigger";
        Description = "No description";
        Settings = new TriggerSettings();
    }

    public abstract Task ExecuteAsync();
}