using StudyOrganizer.Helper.Serializers;

namespace StudyOrganizer.Helper;

public class DataHelper<T>
{
    private readonly ISerializer<T> _serializer;

    public DataHelper(ISerializer<T> serializer)
    {
        _serializer = serializer;
    }

    public T? LoadData()
    {
        return _serializer.Deserialize();
    }

    public void SaveData(T data)
    {
        _serializer.Serialize(data);
    }
}