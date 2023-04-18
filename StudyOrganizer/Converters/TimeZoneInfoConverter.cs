using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyOrganizer.Converters;

public class TimeZoneInfoConverter : JsonConverter<TimeZoneInfo>
{
    public override TimeZoneInfo Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return TimeZoneInfo.FindSystemTimeZoneById(reader.GetString()!);
    }

    public override void Write(
        Utf8JsonWriter writer,
        TimeZoneInfo timeZoneInfo,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(timeZoneInfo.StandardName);
    }
}