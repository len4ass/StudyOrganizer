namespace StudyOrganizer.Extensions;

public static class TimeZoneInfoExtensions
{
    public static (bool, TimeZoneInfo?) TryParse(string timeZoneId)
    {
        TimeZoneInfo timeZoneParsed;
        try
        {
            timeZoneParsed = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return (false, null);
        }
        catch (InvalidTimeZoneException)
        {
            return (false, null);
        }

        return (true, timeZoneParsed);
    }
}