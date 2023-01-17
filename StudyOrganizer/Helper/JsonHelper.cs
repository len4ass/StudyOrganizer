using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace StudyOrganizer.Helper;

public class JsonHelper
{
    private readonly string _path;

    public JsonHelper(string path)
    {
        _path = path;
    }

    public void Serialize<T>(T element)
    {
        using var writer = new StreamWriter(_path, false);
        JsonSerializer.Serialize(
            writer.BaseStream, 
            element, 
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
        
        writer.Close();
    }

    public T? Deserialize<T>()
    {
        using var reader = new StreamReader(_path);

        var response = JsonSerializer.Deserialize<T>(reader.BaseStream);
        reader.Close();

        return response;
    }
}