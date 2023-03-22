using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Models.Link;

namespace StudyOrganizer.Repositories.Link;

public class LinkInfoRepository : ILinkInfoRepository
{
    private readonly MyDbContext _dbContext;

    public LinkInfoRepository(MyDbContext myDbContext)
    {
        _dbContext = myDbContext;
    }

    public async Task AddAsync(LinkInfo element)
    {
        await _dbContext.Links.AddAsync(element);
    }

    public async Task RemoveAsync(LinkInfo element)
    {
        var deadline = await FindAsync(element.Name);
        if (deadline is not null)
        {
            _dbContext.Links.Remove(deadline);
        }
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public Task ClearAllAsync()
    {
        _dbContext.Links.Clear();
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<LinkInfo>> GetDataAsync()
    {
        return await _dbContext.Links.ToListAsync();
    }

    public async Task<LinkInfo?> FindAsync(string name)
    {
        return await _dbContext.Links.FindAsync(name);
    }
}