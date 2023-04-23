namespace StudyOrganizer.Helper;

public static class RegexHelper
{
    public const string DateTimeRegex =
        @"(?<day>\d{2})\.(?<month>\d{2})\.(?<year>\d{4})\s(?<hour>\d{2}):(?<minute>\d{2})(:(?<second>\d{2}))?";
}