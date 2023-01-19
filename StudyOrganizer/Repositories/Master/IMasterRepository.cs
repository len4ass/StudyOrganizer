namespace StudyOrganizer.Repositories.Master;

public interface IMasterRepository : IRepository, IDataProvider<NameRepositoryPair>, IFindable<string, IRepository?>
{
    
}