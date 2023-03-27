using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Models.Deadline;

namespace StudyOrganizer.Repositories.Deadline;

public class DeadlineInfoRepository : IDeadlineInfoRepository
{
    private readonly MyDbContext _dbContext;

    public DeadlineInfoRepository(MyDbContext myDbContext)
    {
        _dbContext = myDbContext;
    }

    public async Task AddAsync(DeadlineInfo element)
    {
        await _dbContext.Deadlines.AddAsync(element);
    }

    public async Task RemoveAsync(DeadlineInfo element)
    {
        var deadline = await FindAsync(element.Name);
        if (deadline is not null)
        {
            _dbContext.Deadlines.Remove(deadline);
        }
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public Task ClearAllAsync()
    {
        _dbContext.Deadlines.Clear();
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<DeadlineInfo>> GetDataAsync()
    {
        return await _dbContext.Deadlines.ToListAsync();
    }

    public async Task<DeadlineInfo?> FindAsync(string name)
    {
        return await _dbContext.Deadlines.FindAsync(name);
    }

    public async Task<DeadlineInfo?> FindByPredicateAsync(Expression<Func<DeadlineInfo, bool>> predicate)
    {
        return await _dbContext.Deadlines.FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<DeadlineInfo>> FilterByPredicateAsync(Expression<Func<DeadlineInfo, bool>> predicate)
    {
        return await _dbContext.Deadlines.Where(predicate).ToListAsync();
    }
}