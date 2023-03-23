using StudyOrganizer.Models.Command;

namespace StudyOrganizer.Repositories.BotCommand;

public interface ICommandInfoRepository : 
    IRepository, 
    IDataProvider<CommandInfo>, 
    IFindable<string, CommandInfo?>,
    IPredicateFindable<CommandInfo>
{
    
}