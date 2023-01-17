using System.Runtime.CompilerServices;

namespace StudyOrganizer.Object;

public class Link
{
    public string Name { get; init; }
    public string Description { get; init; }
    public string Uri { get; init; }

    public Link(string name, string description, string uri)
    {
        Name = name;
        Description = description;
        Uri = uri;
    }

    public override bool Equals(object? obj)
    {
        var link = obj as Link;
        if (link is null)
        {
            return false;
        }
        
        return Equals(link);
    }

    private bool Equals(Link other)
    {
        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Name.GetHashCode();
            hashCode = (hashCode * 397) ^ Description.GetHashCode();
            hashCode = (hashCode * 397) ^ Uri.GetHashCode();
            return hashCode;
        }
    }
}