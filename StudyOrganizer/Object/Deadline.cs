namespace StudyOrganizer.Object;

public class Deadline
{
    public string Name { get; init; }
    
    public string Description { get; init; }
    
    public DateTime Date { get; init; }
    
    public Deadline(string name, string description, DateTime date)
    {
        Name = name;
        Description = description;
        Date = date;
    }

    public override bool Equals(object? obj)
    {
        var deadline = obj as Deadline;
        if (deadline is null)
        {
            return false;
        }
        
        return Equals(deadline);
    }

    private bool Equals(Deadline other)
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