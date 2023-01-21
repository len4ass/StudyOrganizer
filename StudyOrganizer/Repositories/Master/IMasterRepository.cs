namespace StudyOrganizer.Repositories.Master;

public interface IMasterRepository
{
    public bool Add(string name, IRepository repository);

    public IRepository? Find(string name);
}