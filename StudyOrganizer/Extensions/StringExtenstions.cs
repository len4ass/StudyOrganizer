namespace StudyOrganizer.Extensions;

public static class StringExtenstions
{
    public static string RemoveAllHtmlCharacters(this string source)
    {
        return source.Replace("&", "")
            .Replace("<", "")
            .Replace(">", "")
            .Replace("\"", "")
            .Replace("'", "");
    }
}