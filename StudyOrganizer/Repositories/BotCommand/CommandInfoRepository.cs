using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Models.Command;

namespace StudyOrganizer.Repositories.BotCommand;

public class CommandInfoRepository : ICommandInfoRepository
{
    private readonly MyDbContext _dbContext;

    public CommandInfoRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(CommandInfo data)
    {
        await _dbContext.AddAsync(data);
    }

    public Task RemoveAsync(CommandInfo data)
    {
        throw new NotSupportedException();
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public Task ClearAllAsync()
    {
        _dbContext.Commands.Clear();
        return Task.CompletedTask;
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