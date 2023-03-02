using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Database;
using StudyOrganizer.Models.Trigger;

namespace StudyOrganizer.Repositories.SimpleTrigger;

public class SimpleTriggerRepository : ISimpleTriggerRepository
{
    private readonly MyDbContext _dbContext;

    public SimpleTriggerRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public Task AddAsync(TriggerInfo data)
    {
        throw new NotSupportedException();
    }
    
    public Task RemoveAsync(TriggerInfo data)
    {
        throw new NotSupportedException();
    }
    
    public Task SaveAsync()
    {
        throw new NotSupportedException();
    }

    public async Task<IReadOnlyList<TriggerInfo>> GetDataAsync()
    {
        return await _dbContext.Triggers.ToListAsync();
    }

    public async Task<TriggerInfo?> FindAsync(string element)
    {
        return await _dbContext.Triggers.FindAsync(element);
    }
}