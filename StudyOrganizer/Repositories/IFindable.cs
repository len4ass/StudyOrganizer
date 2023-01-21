namespace StudyOrganizer.Repositories;

public interface IFindable<in TInput, TOutput>
{
    Task<TOutput> FindAsync(TInput element);
}