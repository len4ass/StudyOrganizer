using System.Linq.Expressions;
using StudyOrganizer.Models.User;

namespace StudyOrganizer.Repositories.User;

public interface IUserInfoRepository : 
    IRepository, 
    IDataProvider<UserInfo>, 
    IFindable<long, UserInfo?>, 
    IFindable<string, UserInfo?>, 
    IPredicateFindable<UserInfo>,
    ICountable,
    ISkippable<UserInfo>,
    IFilterable<UserInfo>
{
}