using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Models.Trigger;

namespace StudyOrganizer.Repositories.SimpleTrigger;

public class SimpleTriggerRepository : ISimpleTriggerRepository
{
    private readonly MyDbContext _dbContext;

    public SimpleTriggerRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddAsync(TriggerInfo data)
    {
        await _dbContext.Triggers.AddAsync(data);
    }
    
    public Task RemoveAsync(TriggerInfo data)
    {
        throw new NotSupportedException();
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public Task ClearAllAsync()
    {
        _dbContext.Triggers.Clear();
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<TriggerInfo>> GetDataAsync()
    {
        return await _dbContext.Triggers.ToListAsync();
    }

    public async Task<TriggerInfo?> FindAsync(string element)
    {
        return await _dbContext.Triggers.FindAsync(element);
    }

    public async Task<TriggerInfo?> FindByPredicateAsync(Expression<Func<TriggerInfo, bool>> predicate)
    {
        return await _dbContext.Triggers.FirstOrDefaultAsync(predicate);
    }
}