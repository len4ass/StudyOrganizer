using StudyOrganizer.Models.User;

namespace StudyOrganizer.Repositories.User;

public class UserInfoRepository : IUserInfoRepository
{
    private IList<UserInfo> _userData;

    public UserInfoRepository(IList<UserInfo> userData)
    {
        _userData = userData;
    }

    public bool Add(UserInfo element)
    {
        if (_userData.Contains(element))
        {
            return false;
        }
        
        _userData.Add(element);
        return true;
    }

    public bool Remove(UserInfo element)
    {
        return _userData.Remove(element);
    }

    public IReadOnlyList<UserInfo> GetData()
    {
        return _userData.ToList().AsReadOnly();
    }

    public UserInfo? Find(long id)
    {
        return _userData.FirstOrDefault(user => user.Id == id);
    }
}