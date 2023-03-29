using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StudyOrganizer.Extensions;

namespace StudyOrganizer.Models.Deadline;

public class DeadlineInfo
{
    [Key]
    public string Name { get; init; }
    
    public string Description { get; set; }
    
    public DateTimeOffset DateUtc { get; set; }

    public DeadlineInfo()
    {
    }
    
    public DeadlineInfo(string name, string description, DateTimeOffset dateUtc)
    {
        Name = name;
        Description = description;
        DateUtc = dateUtc;
    }

    public string ToString(TimeZoneInfo timeZoneInfo)
    {
        var stringDays = DateUtc.GetDaysDifferenceUtc().GetStringDays();
        var stringHours = DateUtc.GetHourDifferenceUtc().GetStringHours();
        var stringMinutes = DateUtc.GetMinuteDifferenceUtc().GetStringMinutes();

        var dateTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(DateUtc.DateTime, timeZoneInfo);
        return $"{Description} — " +
               $"<b>{dateTimeLocal.ToString("g", new CultureInfo("ru-RU"))} " +
               $"(Осталось: {stringDays} {stringHours} {stringMinutes})</b> " +
               $"({Name})";
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
        return Name.GetHashCode();
    }
}