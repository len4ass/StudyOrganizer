using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Database;
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

    public async Task<IReadOnlyList<DeadlineInfo>> GetDataAsync()
    {
        return await _dbContext.Deadlines.ToListAsync();
    }

    public async Task<DeadlineInfo?> FindAsync(string name)
    {
        return await _dbContext.Deadlines.FirstOrDefaultAsync(deadline => deadline.Name == name);
    }
}