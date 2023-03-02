using System.Text.Json;
using Serilog;
using StudyOrganizer.Helper;
using StudyOrganizer.Helper.Serializers;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Services.TriggerService.Jobs;
using StudyOrganizer.Settings;
using Telegram.Bot;
using IJob = StudyOrganizer.Services.TriggerService.Jobs.IJob;

namespace StudyOrganizer.Loaders;

public class CronJobLoader
{
    private bool _isLoaded;
    
    private readonly string _triggersDirPath;
    private readonly string _triggersSettingsDirPath;
    
    private readonly IMasterRepository _masterRepository;
    private readonly GeneralSettings _generalSettings;
    private readonly ITelegramBotClient _botClient;
    
    private readonly IDictionary<string, IJob> _jobImplementations 
        = new Dictionary<string, IJob>(); 

    public CronJobLoader(
        IMasterRepository masterRepository, 
        GeneralSettings generalSettings, 
        ITelegramBotClient botClient, 
        string triggersDirPath,
        string triggersSettingsDirPath)
    {
        _masterRepository = masterRepository;
        _generalSettings = generalSettings;
        _botClient = botClient;
        _triggersDirPath = triggersDirPath;
        _triggersSettingsDirPath = triggersSettingsDirPath;
    }
    
    private TriggerSettings? GetCronTriggerSettings(string triggerName)
    {
        string pathToFile = Path.Combine(_triggersSettingsDirPath, triggerName + ".json");
        if (!File.Exists(pathToFile))
        {
            return null;
        }

        var loader = new DataHelper<TriggerSettings>(new JsonHelper<TriggerSettings>(pathToFile));
        try
        {
            return loader.LoadData();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private void LoadJobs()
    {
        if (_isLoaded)
        {
            return;
        }
        
        var directory = new DirectoryInfo(_triggersDirPath);
        var files = directory.GetFiles("*.dll");
        foreach (var file in files)
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
                _botClient,
                _generalSettings);
                
            if (instance is not SimpleTrigger cronTrigger)
            {
                continue;
            }
            
            var settings = GetCronTriggerSettings(cronTrigger.Name);
            if (settings is not null)
            {
                var changes = ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(
                    settings, 
                    cronTrigger);
                foreach (var change in changes)
                {
                    Log.Logger.Information(
                        $"Изменены настройки команды {cronTrigger.Name} при загрузке: " +
                        $"значение {change.Name} изменено с {change.PreviousValue} на {change.CurrentValue}");
                }
            }
            
            _jobImplementations[cronTrigger.Name] = new CronJob(cronTrigger);
            Log.Logger.Information($"Триггер {cronTrigger.Name} загружен.");
        }

        _isLoaded = true;
    }
    

    public IDictionary<string, IJob> GetTriggerImplementations()
    {
        LoadJobs();

        return _jobImplementations;
    }
}