using StudyOrganizer.Models.User;

namespace StudyOrganizer.Repositories.User;

public interface IUserInfoRepository : 
    IRepository, IDataProvider<UserInfo>, IFindable<long, UserInfo?>, IFindable<string, UserInfo?>
{
    
}