using System.Text;

namespace StudyOrganizer.Formatters;

public static class TextFormatter
{
    public static string BuildHtmlStringFromList<T>(IList<T> list, string optionalString = "")
    {
        var sb = new StringBuilder();
        for (var i = 0; i < list.Count; i++)
        {
            sb.AppendLine($"{optionalString}<b>{i + 1}</b>. {list[i]}");
        }

        return sb.ToString();
    }
}