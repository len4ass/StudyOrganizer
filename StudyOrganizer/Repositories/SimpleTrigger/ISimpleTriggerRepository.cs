using StudyOrganizer.Models.Trigger;

namespace StudyOrganizer.Repositories.SimpleTrigger;

public interface ISimpleTriggerRepository : 
    IRepository, 
    IDataProvider<TriggerInfo>, 
    IFindable<string, TriggerInfo?>, 
    IPredicateFindable<TriggerInfo>
{

}