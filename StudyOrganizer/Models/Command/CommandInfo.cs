using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using StudyOrganizer.Models.User;
using StudyOrganizer.Settings;

namespace StudyOrganizer.Models.Command;

public class CommandInfo
{
    [Key]
    public string Name { get; init; }
    public string Description { get; init; }
    
    public string Format { get; init; }
    
    public CommandSettings Settings { get; set; }

    public CommandInfo()
    {
    }
    
    public CommandInfo(string name, string description, string format, CommandSettings settings)
    {
        Name = name;
        Description = description;
        Format = format;
        Settings = settings;
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