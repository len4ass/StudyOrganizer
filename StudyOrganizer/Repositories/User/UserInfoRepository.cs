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
        var users = await GetDataAsync();
        return users.FirstOrDefault(user => user.Handle == handle);
    }
}