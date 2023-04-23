namespace StudyOrganizer.Helper.Serializers;

public interface ISerializer<T>
{
    public Task SerializeAsync(T obj);

    public Task<T?> DeserializeAsync();

    public void Serialize(T obj);

    public T? Deserialize();
}