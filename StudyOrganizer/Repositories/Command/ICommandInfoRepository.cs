using StudyOrganizer.Models.Command;

namespace StudyOrganizer.Repositories.Command;

public interface ICommandInfoRepository : IRepository, IDataProvider<CommandInfo>, IFindable<string, CommandInfo?>
{
    
}