namespace StudyOrganizer.Data.Interface;

public interface IDataProvider<T>
{
    public void Add(T data);

    public void Remove(T data);

    public IReadOnlyList<T>? GetData();
}