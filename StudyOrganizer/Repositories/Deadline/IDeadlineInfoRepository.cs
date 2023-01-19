using StudyOrganizer.Models.Deadline;

namespace StudyOrganizer.Repositories.Deadline;

public interface IDeadlineInfoRepository : IRepository, IDataProvider<DeadlineInfo>, IFindable<string, DeadlineInfo?>
{
    
}