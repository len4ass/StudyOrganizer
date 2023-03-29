using System.ComponentModel.DataAnnotations;

namespace StudyOrganizer.Models.Link;

public class LinkInfo
{
    [Key]
    public string Name { get; init; }
    public string Description { get; set; }
    public string Uri { get; set; }

    public LinkInfo()
    {
    }

    public LinkInfo(string name, string description, string uri)
    {
        Name = name;
        Description = description;
        Uri = uri;
    }

    public override string ToString()
    {
        return $"<a href=\"{Uri}\"><b>{Name}</b></a> — {Description}";
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
        return Name.GetHashCode();
    }
}