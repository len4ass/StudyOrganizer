namespace StudyOrganizer.Repositories;

public interface IDataProvider<T>
{
    Task AddAsync(T data);

    Task RemoveAsync(T data);
    
    Task<IReadOnlyList<T>> GetDataAsync();
}