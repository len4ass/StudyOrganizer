using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        await _dbContext.Commands.AddAsync(data);
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

    public async Task<CommandInfo?> FindByPredicateAsync(Expression<Func<CommandInfo, bool>> predicate)
    {
        return await _dbContext.Commands.FirstOrDefaultAsync(predicate);
    }
}