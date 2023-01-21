using StudyOrganizer.Models.Command;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;

namespace StudyOrganizer.Loaders;

public class CommandLoader
{
    private bool _commandsLoaded;
    
    private readonly string _path;
    
    private readonly IMasterRepository _masterRepository;
    private readonly GeneralSettings _generalSettings;
    
    private readonly IList<CommandInfo> _commandData = new List<CommandInfo>();
    private readonly IDictionary<string, BotCommand> _commandImplementations 
        = new Dictionary<string, BotCommand>(); 

    public CommandLoader(IMasterRepository masterRepository, GeneralSettings generalSettings, string path)
    {
        _masterRepository = masterRepository;
        _generalSettings = generalSettings;
        _path = path;
    }

    public void LoadCommands()
    {
        if (_commandsLoaded)
        {
            return;
        }
        
        var directory = new DirectoryInfo(_path);
        var files = directory.GetFiles("*.dll");
        foreach (var file in files)
        {
            var path = file.FullName;
            var type = AssemblyLoader.GetTypeFromAssembly(path);
            if (type is null)
            {
                continue;
            }
            
            var instance = AssemblyLoader.CreateTypeInstance(
                type, 
                _masterRepository, 
                _generalSettings);
                
            if (instance is not BotCommand botCommand)
            {
                continue;
            }

            var commandInfo = new CommandInfo(
                botCommand.Name,
                botCommand.Description,
                botCommand.AccessLevel);
                
            _commandData.Add(commandInfo);
            _commandImplementations[botCommand.Name] = botCommand;
        }

        _commandsLoaded = true;
    }

    public void ReloadCommands()
    {
        _commandsLoaded = false;
        _commandData.Clear();
        _commandImplementations.Clear();
        LoadCommands();
    }

    public IList<CommandInfo> GetCommandInfoData()
    {
        LoadCommands();

        return _commandData;
    }

    public IDictionary<string, BotCommand> GetCommandImplementations()
    {
        LoadCommands();

        return _commandImplementations;
    }
}