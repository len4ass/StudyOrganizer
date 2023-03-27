using System.ComponentModel.DataAnnotations;

namespace StudyOrganizer.Models.User;

public sealed class UserInfo
{
    [Key]
    public long Id { get; init; }
    public string Name { get; set; }
    public string? Handle { get; set; }
    public long MsgAmount { get; set; }
    public AccessLevel Level { get; set; }
    public bool CoolestOfTheDay { get; set; }
    public int WonCOTD { get; set; }
    public DateTimeOffset BirthdayUtc { get; set; }

    public UserInfo()
    {
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not UserInfo user)
        {
            return false;
        }
        
        return Equals(user);
    }

    private bool Equals(UserInfo other)
    {
        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        var hashCode = Id.GetHashCode();
        return hashCode;
    }
}