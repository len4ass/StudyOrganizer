using System.ComponentModel.DataAnnotations;

namespace StudyOrganizer.Models.Trigger;

public class TriggerInfo
{
    [Key]
    public string Name { get; init; }
    public string Description { get; init; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public int Second { get; set; }
    public int RunEveryGivenSeconds { get; set; }
    public bool ShouldRun { get; set; }

    public TriggerInfo()
    {
    }
    
    public TriggerInfo(
        string name, 
        string description, 
        int hour = 0, 
        int minute = 0, 
        int second = 0, 
        int runEveryGivenSeconds = 0, 
        bool shouldRun = false)
    {
        Name = name;
        Description = description;
        Hour = hour;
        Minute = minute;
        Second = second;
        RunEveryGivenSeconds = runEveryGivenSeconds;
        ShouldRun = shouldRun;
    }
    
    public override string ToString()
    {
        return $"{Name}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TriggerInfo triggerInfo)
        {
            return false;
        }
        
        return Equals(triggerInfo);
    }

    private bool Equals(TriggerInfo other)
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