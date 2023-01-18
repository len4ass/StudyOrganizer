using StudyOrganizer.Models.User;

namespace StudyOrganizer.Repositories.User;

public class UserInfoRepository : IRepository, IDataProvider<UserInfo>, IFindable<long, UserInfo?>
{
    private IList<UserInfo> _userData;

    public UserInfoRepository(IList<UserInfo> userData)
    {
        _userData = userData;
    }

    public bool Add(UserInfo element)
    {
        throw new NotImplementedException();
    }

    public bool Remove(UserInfo element)
    {
        throw new NotImplementedException();
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