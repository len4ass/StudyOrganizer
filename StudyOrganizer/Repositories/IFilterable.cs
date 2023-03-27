using System.Linq.Expressions;

namespace StudyOrganizer.Repositories;

public interface IFilterable<T>
{
    Task<IEnumerable<T>> FilterByPredicateAsync(Expression<Func<T, bool>> predicate);
}