namespace StudyOrganizer.Repositories.Master;

public class MasterRepository : IMasterRepository
{
    private readonly IDictionary<string, IRepository> _repositories;

    public MasterRepository()
    {
        _repositories = new Dictionary<string, IRepository>();
    }

    public bool Add(NameRepositoryPair data)
    {
        return _repositories.TryAdd(data.Name, data.Repository);
    }

    public bool Remove(NameRepositoryPair data)
    {
        throw new NotSupportedException();
    }

    public IReadOnlyList<NameRepositoryPair> GetData()
    {
        return _repositories
            .Select(pair => new NameRepositoryPair(pair.Key, pair.Value))
            .ToList()
            .AsReadOnly();
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