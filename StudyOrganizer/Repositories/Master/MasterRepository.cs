namespace StudyOrganizer.Repositories.Master;

public class MasterRepository : IMasterRepository
{
    private readonly IDictionary<string, IRepository> _repositories;

    public MasterRepository()
    {
        _repositories = new Dictionary<string, IRepository>();
    }

    public bool Add(string name, IRepository repository)
    {
        return _repositories.TryAdd(name, repository);
    }
    
    public IRepository? Find(string element)
    {
        if (!_repositories.ContainsKey(element))
        {
            return null;
        }

        return _repositories[element];
    }
}