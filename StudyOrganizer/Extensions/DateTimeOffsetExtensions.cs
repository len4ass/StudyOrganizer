namespace StudyOrganizer.Extensions;

public static class DateTimeOffsetExtensions
{
    public static bool IsAfter(this DateTimeOffset dateTime, DateTimeOffset other)
    {
        return dateTime > other;
    }
    
    public static int GetDaysDifferenceUtc(this DateTimeOffset date)
    {
        return (int)(date - DateTimeOffset.UtcNow).TotalDays;
    }

    public static int GetHourDifferenceUtc(this DateTimeOffset date)
    {
        int hours = 24 * GetDaysDifferenceUtc(date);
        return (int)(date - DateTimeOffset.UtcNow).TotalHours - hours;
    }
        
    public static int GetMinuteDifferenceUtc(this DateTimeOffset date)
    {
        int minutes = 60 * (int)(date - DateTimeOffset.UtcNow).TotalHours;
        return (int)(date - DateTimeOffset.UtcNow).TotalMinutes - minutes;
    }
}