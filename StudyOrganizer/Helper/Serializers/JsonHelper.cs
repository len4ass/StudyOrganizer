using System.Text.Json;

namespace StudyOrganizer.Helper.Serializers;

public class JsonHelper<T> : ISerializer<T>
{
    private readonly string _path;

    public JsonHelper(string path)
    {
        _path = path;
    }

    public void Serialize(T element)
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

    public T? Deserialize()
    {
        using var reader = new StreamReader(_path);

        var response = JsonSerializer.Deserialize<T>(reader.BaseStream);
        reader.Close();

        return response;
    }
}