namespace StudyOrganizer.Extensions;

public static class IntegerToStringExtensions
{
    public static string GetStringYears(this int years)
    {
        return $"{years}л.";
    }
        
    public static string GetStringDays(this int days)
    {
        return $"{days}д.";
    }
        
    public static string GetStringHours(this int hours)
    {
        return $"{hours}ч.";
    }
        
    public static string GetStringMinutes(this int minutes)
    {
        return $"{minutes}мин.";
    } 
}