namespace StudyOrganizer.Repositories;

public interface IDataProvider<T>
{
    public bool Add(T data);

    public bool Remove(T data);

    public IReadOnlyList<T> GetData();
}