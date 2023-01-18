namespace StudyOrganizer.Repositories;

public interface IFindable<in TInput, out TOutput>
{
    public TOutput Find(TInput element);
}