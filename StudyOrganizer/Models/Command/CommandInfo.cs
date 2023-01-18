using StudyOrganizer.Enum;

namespace StudyOrganizer.Models.Command;

public class CommandInfo
{
    public string Name { get; init; }
    public string Description { get; init; }
    public AccessLevel AccessLevel { get; init; }
    
    public CommandInfo(string name, string description, AccessLevel accessLevel)
    {
        Name = name;
        Description = description;
        AccessLevel = accessLevel;
    }

    public override string ToString()
    {
        return $"/{Name}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CommandInfo command)
        {
            return false;
        }
        
        return Equals(command);
    }

    private bool Equals(CommandInfo other)
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