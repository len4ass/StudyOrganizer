using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Database;
using StudyOrganizer.Models.Command;

namespace StudyOrganizer.Repositories.Command;

public class CommandInfoRepository : ICommandInfoRepository
{
    private readonly MyDbContext _dbContext;

    public CommandInfoRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(CommandInfo data)
    {
        throw new NotSupportedException();
    }

    public Task RemoveAsync(CommandInfo data)
    {
        throw new NotSupportedException();
    }

    public Task SaveAsync()
    {
        throw new NotSupportedException();
    }

    public async Task<IReadOnlyList<CommandInfo>> GetDataAsync()
    {
        return await _dbContext.Commands.ToListAsync();
    }

    public async Task<CommandInfo?> FindAsync(string name)
    {
        return await _dbContext.Commands.FindAsync(name);
    }
}