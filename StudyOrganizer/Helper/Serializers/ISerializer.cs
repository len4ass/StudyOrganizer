namespace StudyOrganizer.Helper.Serializers;

public interface ISerializer<T>
{
    public void Serialize(T obj);

    public T? Deserialize();
}