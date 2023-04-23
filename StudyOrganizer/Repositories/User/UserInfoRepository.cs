using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Models.User;

namespace StudyOrganizer.Repositories.User;

public class UserInfoRepository : IUserInfoRepository
{
    private readonly MyDbContext _dbContext;

    public UserInfoRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public int Count()
    {
        return _dbContext.Users.Count();
    }
    
    public async Task AddAsync(UserInfo element)
    {
        await _dbContext.Users.AddAsync(element);
    }

    public async Task RemoveAsync(UserInfo element)
    {
        var user = await FindAsync(element.Id);
        if (user is not null)
        {
            _dbContext.Users.Remove(user);
        }
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public Task ClearAllAsync()
    {
        _dbContext.Users.Clear();
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<UserInfo>> GetDataAsync()
    {
        return await _dbContext.Users.ToListAsync();
    }

    public async Task<UserInfo?> FindAsync(long id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task<UserInfo?> FindAsync(string handle)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(user => user.Handle == handle);
    }

    public async Task<UserInfo?> FindByPredicateAsync(Expression<Func<UserInfo, bool>> predicate)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(predicate);
    }

    public async Task<UserInfo> SkipAndTakeFirst(int skipCount)
    {
        if (skipCount >= _dbContext.Users.Count())
        {
            throw new ArgumentException(
                "Аргумент не может быть больше количества вхождений в базе данных",
                nameof(skipCount));
        }
        
        var users = await _dbContext.Users.Skip(skipCount).Take(1).ToListAsync();
        return users[0];
    }

    public async Task<IEnumerable<UserInfo>> FilterByPredicateAsync(Expression<Func<UserInfo, bool>> predicate)
    {
        return await _dbContext.Users.Where(predicate).ToListAsync();
    }
}