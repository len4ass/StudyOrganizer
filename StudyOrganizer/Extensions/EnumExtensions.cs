using System.Text;

namespace StudyOrganizer.Extensions;

public static class EnumExtensions
{
    public static string BuildHtmlStringFromEnum<T>(string optionalString = "")
        where T : Enum
    {
        var sb = new StringBuilder();
        var enumValues = Enum.GetNames(typeof(T));
        for (var i = 0; i < enumValues.Length; i++)
        {
            sb.AppendLine($"{optionalString}<b>{i + 1}</b>. {enumValues[i]}");
        }

        return sb.ToString();
    }

    public static string BuildOptionalSlashStringFromEnum<T>()
        where T : Enum
    {
        var sb = new StringBuilder();
        var enumValues = Enum.GetNames(typeof(T));
        for (var i = 0; i < enumValues.Length; i++)
        {
            if (i == enumValues.Length - 1)
            {
                sb.Append(enumValues[i]);
            }
            else
            {
                sb.Append($"{enumValues[i]}/");
            }
        }

        return sb.ToString();
    }
}