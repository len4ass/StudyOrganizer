using System.Globalization;

namespace StudyOrganizer.Parsers;

public static class TextParser
{
    public static IList<string> ParseMessageToCommand(string message)
    {
        message = message.Trim();
        if (!message.StartsWith('/'))
        {
            return Enumerable.Empty<string>()
                .ToList();
        }

        message = message.TrimStart('/');
        var entries = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var atIndex = entries[0]
            .IndexOf('@');
        if (atIndex >= 0)
        {
            entries[0] = entries[0][..atIndex];
        }

        return entries;
    }

    public static DateTimeOffset? ParseDateTime(string dateTimeString, TimeZoneInfo timeZoneInfo)
    {
        var parsed = DateTime.TryParse(
            dateTimeString,
            new CultureInfo("ru-RU"),
            DateTimeStyles.None,
            out var dateTime);
        if (!parsed)
        {
            return null;
        }

        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZoneInfo);
        return new DateTimeOffset(utcDateTime, TimeSpan.Zero);
    }
}