using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Database;
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

    public async Task<IReadOnlyList<UserInfo>> GetDataAsync()
    {
        return await _dbContext.Users.ToListAsync();
    }

    public async Task<UserInfo?> FindAsync(long id)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id);
    }

    public async Task<UserInfo?> FindAsync(string handle)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(user => user.Handle == handle);
    }
}