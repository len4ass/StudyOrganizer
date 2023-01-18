namespace StudyOrganizer.Models.Deadline;

public class DeadlineInfo
{
    public string Name { get; init; }
    
    public string Description { get; init; }
    
    public DateTime Date { get; init; }
    
    public DeadlineInfo(string name, string description, DateTime date)
    {
        Name = name;
        Description = description;
        Date = date;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not DeadlineInfo deadline)
        {
            return false;
        }
        
        return Equals(deadline);
    }

    private bool Equals(DeadlineInfo other)
    {
        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Name.GetHashCode();
            hashCode = (hashCode * 397) ^ Description.GetHashCode();
            hashCode = (hashCode * 397) ^ Date.GetHashCode();
            return hashCode;
        }
    }
}