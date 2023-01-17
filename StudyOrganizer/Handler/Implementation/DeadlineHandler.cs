using StudyOrganizer.Data.Interface;
using StudyOrganizer.Handler.Interface;
using StudyOrganizer.Object;

namespace StudyOrganizer.Handler.Implementation;

public class DeadlineHandler : IDeadlineHandler
{
    private IDataProvider<Deadline> _deadlineData;

    public DeadlineHandler(IDataProvider<Deadline> deadlineData)
    {
        _deadlineData = deadlineData;
    }
}