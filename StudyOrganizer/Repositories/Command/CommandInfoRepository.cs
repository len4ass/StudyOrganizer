using StudyOrganizer.Models.Command;

namespace StudyOrganizer.Repositories.Command;

public class CommandInfoRepository : ICommandInfoRepository
{
    private IList<CommandInfo> _commandData;

    public CommandInfoRepository(IList<CommandInfo> commandData)
    {
        _commandData = commandData;
    }

    public bool Add(CommandInfo data)
    {
        throw new NotSupportedException();
    }

    public bool Remove(CommandInfo data)
    {
        throw new NotSupportedException();
    }

    public IReadOnlyList<CommandInfo> GetData()
    {
        return _commandData.ToList().AsReadOnly();
    }

    public CommandInfo? Find(string name)
    {
        return _commandData.FirstOrDefault(command => command.Name == name);
    }
}