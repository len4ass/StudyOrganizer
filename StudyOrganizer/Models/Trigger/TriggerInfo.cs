using System.ComponentModel.DataAnnotations;
using StudyOrganizer.Settings;

namespace StudyOrganizer.Models.Trigger;

public class TriggerInfo
{
    [Key]
    public string Name { get; init; }
    public string Description { get; init; }

    public TriggerSettings Settings { get; set; }

    public TriggerInfo()
    {
    }
    
    public TriggerInfo(
        string name, 
        string description, 
        TriggerSettings settings)
    {
        Name = name;
        Description = description;
        Settings = settings;
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