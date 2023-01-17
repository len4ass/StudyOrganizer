namespace StudyOrganizer.Object;

public class Command
{
    public string Name { get; init; }
    public string Description { get; init; }
    
    public Command(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public override string ToString()
    {
        return $"{Name}";
    }

    public override bool Equals(object? obj)
    {
        var command = obj as Command;
        if (command is null)
        {
            return false;
        }
        
        return Equals(command);
    }

    private bool Equals(Command other)
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