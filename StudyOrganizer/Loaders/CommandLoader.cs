using System.Text.Json;
using Serilog;
using StudyOrganizer.Helper;
using StudyOrganizer.Helper.Serializers;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Services.BotService.Command;
using StudyOrganizer.Settings;

namespace StudyOrganizer.Loaders;

public class CommandLoader
{
    private bool _isLoaded;
    
    private readonly string _commandDirPath;
    private readonly string _commandSettingsDirPath;
    
    private readonly IMasterRepository _masterRepository;
    private readonly GeneralSettings _generalSettings;
    private readonly IDictionary<string, BotCommand> _commandImplementations 
        = new Dictionary<string, BotCommand>(); 

    public CommandLoader(
        IMasterRepository masterRepository, 
        GeneralSettings generalSettings, 
        string commandDirPath,
        string commandSettingsDirPath)
    {
        _masterRepository = masterRepository;
        _generalSettings = generalSettings;
        _commandDirPath = commandDirPath;
        _commandSettingsDirPath = commandSettingsDirPath;
    }

    private CommandSettings? GetCommandSettings(string commandName)
    {
        string pathToFile = Path.Combine(_commandSettingsDirPath, commandName + ".json");
        if (!File.Exists(pathToFile))
        {
            return null;
        }

        var loader = new DataHelper<CommandSettings>(new JsonHelper<CommandSettings>(pathToFile));
        try
        {
            return loader.LoadData();
        }
        catch (JsonException)
        {
            return null;
        }
    }
    
    private void LoadCommands()
    {
        if (_isLoaded)
        {
            return;
        }
        
        var commandDirectory = new DirectoryInfo(_commandDirPath);
        var commandDllFiles = commandDirectory.GetFiles("*.dll");
        foreach (var file in commandDllFiles)
        {
            var path = file.FullName;
            var type = ReflectionHelper.GetTypeFromAssembly(path);
            if (type is null)
            {
                continue;
            }
            
            var instance = ReflectionHelper.CreateTypeInstance(
                type, 
                _masterRepository, 
                _generalSettings);
                
            if (instance is not BotCommand botCommand)
            {
                continue;
            }
            
            var settings = GetCommandSettings(botCommand.Name);
            if (settings is not null)
            {
                var changes = ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(
                    settings, 
                    botCommand);
                foreach (var change in changes)
                {
                    Log.Logger.Information(
                        $"Изменены настройки команды {botCommand.Name} при загрузке: " +
                        $"значение {change.Name} изменено с {change.PreviousValue} на {change.CurrentValue}");
                }
            }

            _commandImplementations[botCommand.Name] = botCommand;
            Log.Logger.Information($"Команда {botCommand.Name} загружена.");
        }

        _isLoaded = true;
    }

    public IDictionary<string, BotCommand> GetCommandImplementations()
    {
        LoadCommands();

        return _commandImplementations;
    }
}