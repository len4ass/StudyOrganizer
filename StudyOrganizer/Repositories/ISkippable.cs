namespace StudyOrganizer.Repositories;

public interface ISkippable<T>
{
    T SkipAndTakeFirst(int skipCount);
}