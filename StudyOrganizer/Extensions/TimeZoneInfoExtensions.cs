namespace StudyOrganizer.Extensions;

public static class TimeZoneInfoExtensions
{
    public static TimeZoneInfo? TryParse(string timeZoneId)
    {
        TimeZoneInfo timeZoneParsed;
        try
        {
            timeZoneParsed = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return null;
        }
        catch (InvalidTimeZoneException)
        {
            return null;
        }

        return timeZoneParsed;
    }
}