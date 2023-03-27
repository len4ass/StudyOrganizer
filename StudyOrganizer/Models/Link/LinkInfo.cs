using System.ComponentModel.DataAnnotations;

namespace StudyOrganizer.Models.Link;

public class LinkInfo
{
    [Key]
    public string Name { get; init; }
    public string Description { get; init; }
    public string Uri { get; init; }

    public LinkInfo()
    {
    }
    
    public LinkInfo(string name, string description, string uri)
    {
        Name = name;
        Description = description;
        Uri = uri;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not LinkInfo link)
        {
            return false;
        }
        
        return Equals(link);
    }

    private bool Equals(LinkInfo other)
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