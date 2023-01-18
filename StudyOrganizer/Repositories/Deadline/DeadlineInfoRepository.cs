using StudyOrganizer.Models.Deadline;

namespace StudyOrganizer.Repositories.Deadline;

public class DeadlineInfoRepository : IRepository, IDataProvider<DeadlineInfo>, IFindable<string, DeadlineInfo?>
{
    private IList<DeadlineInfo> _deadlineData;

    public DeadlineInfoRepository(IList<DeadlineInfo> deadlineData)
    {
        _deadlineData = deadlineData;
    }

    public bool Add(DeadlineInfo element)
    {
        throw new NotImplementedException();
    }

    public bool Remove(DeadlineInfo element)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<DeadlineInfo> GetData()
    {
        return _deadlineData.ToList().AsReadOnly();
    }

    public DeadlineInfo? Find(string element)
    {
        throw new NotImplementedException();
    }
}