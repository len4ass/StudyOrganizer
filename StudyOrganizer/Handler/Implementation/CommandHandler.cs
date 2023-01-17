using StudyOrganizer.Data.Implementation;
using StudyOrganizer.Data.Interface;
using StudyOrganizer.Handler.Interface;
using StudyOrganizer.Object;

namespace StudyOrganizer.Handler.Implementation;

public class CommandHandler : ICommandHandler
{
    private IDataProvider<Command> _commandData;

    public CommandHandler(IDataProvider<Command> commandData)
    {
        _commandData = commandData;
    }
    
    
}