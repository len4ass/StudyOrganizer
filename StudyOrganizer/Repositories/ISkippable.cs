namespace StudyOrganizer.Repositories;

public interface ISkippable<T>
{
    Task<T> SkipAndTakeFirst(int skipCount);
}