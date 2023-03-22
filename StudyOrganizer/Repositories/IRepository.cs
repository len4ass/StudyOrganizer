namespace StudyOrganizer.Repositories;

public interface IRepository
{
    Task SaveAsync();

    Task ClearAllAsync();
}