using StudyOrganizer.Data.Interface;
using StudyOrganizer.Handler.Interface;
using StudyOrganizer.Object;

namespace StudyOrganizer.Handler.Implementation;

public class UserHandler : IUserHandler
{
    private IDataProvider<User> _userData;

    public UserHandler(IDataProvider<User> userData)
    {
        _userData = userData;
    }
}