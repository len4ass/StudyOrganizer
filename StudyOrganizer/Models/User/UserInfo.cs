using StudyOrganizer.Enum;

namespace StudyOrganizer.Models.User;

public sealed class UserInfo
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public string? Handle { get; init; }
    public long MsgAmount { get; init; }
    public AccessLevel Level { get; init; }
    public bool CoolestOfTheDay { get; init; }
    public int WonCOTD { get; init; }
    public DateTime BirthdayUtc { get; init; }

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
        unchecked
        {
            var hashCode = Id.GetHashCode();
            hashCode = (hashCode * 397) ^ (int) Level;
            hashCode = (hashCode * 397) ^ CoolestOfTheDay.GetHashCode();
            hashCode = (hashCode * 397) ^ WonCOTD;
            hashCode = (hashCode * 397) ^ BirthdayUtc.GetHashCode();
            return hashCode;
        }
    }
}