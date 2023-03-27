using StudyOrganizer.Models.Link;

namespace StudyOrganizer.Repositories.Link;

public interface ILinkInfoRepository : 
    IRepository, 
    IDataProvider<LinkInfo>, 
    IFindable<string, LinkInfo?>,
    IPredicateFindable<LinkInfo>,
    IFilterable<LinkInfo>
{
}