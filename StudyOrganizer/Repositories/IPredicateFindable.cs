using System.Linq.Expressions;

namespace StudyOrganizer.Repositories;

public interface IPredicateFindable<T>
{
    Task<T?> FindByPredicateAsync(Expression<Func<T, bool>> predicate);
}