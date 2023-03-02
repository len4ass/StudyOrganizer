using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;
using StudyOrganizer.Database;
using StudyOrganizer.Loaders;
using StudyOrganizer.Settings;

namespace StudyOrganizer.Services.TriggerService.Jobs;

public class CronJobObserverService : IService
{
    private readonly CronJobAggregator _cronJobAggregator;
    private readonly string _observedDirectory;
    private readonly MyDbContext _dbContext;
    private FileSystemWatcher _observer;

    public CronJobObserverService(
        CronJobAggregator cronJobAggregator, 
        MyDbContext dbContext,
        string observedDirectory)
    {
        _cronJobAggregator = cronJobAggregator;
        _dbContext = dbContext;
        _observedDirectory = observedDirectory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartObserving();
        return Task.CompletedTask;
    }

    private void StartObserving()
    {
        _observer = new FileSystemWatcher(_observedDirectory);
        _observer.NotifyFilter = NotifyFilters.LastWrite;

        _observer.Changed += OnChanged;
        _observer.Error += OnError;

        _observer.Filter = "*.json";
        _observer.EnableRaisingEvents = true;
        Log.Logger.Information($"Запущен мониторинг директории {_observedDirectory}");
    }

    private void UpdateBotCommandInfo(string path)
    {
        var triggerName = Path.GetFileNameWithoutExtension(path);
        var job = _cronJobAggregator.JobExists(triggerName);
        if (job is null)
        {
            return;
        }

        var trigger = job.GetInternalTrigger();
        TriggerSettings settings;
        try
        {
            settings = ProgramData.LoadFrom<TriggerSettings>(path);
        }
        catch (JsonException e)
        {
            Log.Logger.Error(e, 
                $"Произошла ошибка при попытке получения новых настроек триггера {trigger.Name}!");
            return;
        }
        catch (ArgumentNullException e)
        {
            Log.Logger.Error(e, $"Не удалось обновить триггер {trigger.Name}!");
            return;
        }
        
        var triggerInDatabase = _dbContext.Commands.Find(trigger.Name);
        var changes = ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(
            settings,
            trigger);
        ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(settings, triggerInDatabase);
        _dbContext.SaveChanges();

        foreach (var change in changes)
        {
            Log.Logger.Information(
                $"Изменены настройки триггера {trigger.Name}: " +
                $"значение {change.Name} изменено с {change.PreviousValue} на {change.CurrentValue}");
        }
    }
    
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }

        if (!File.Exists(e.FullPath))
        {
            return;
        }
        
        Log.Logger.Information($"Замечено изменение по пути {e.FullPath}.");
        UpdateBotCommandInfo(e.FullPath);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Log.Logger.Error(e.GetException(), "Произошла ошибка при мониторинге директории.");
    }
}