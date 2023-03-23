using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Serilog;
using Serilog.Core;
using StudyOrganizer.Database;
using StudyOrganizer.Loaders;
using StudyOrganizer.Repositories.SimpleTrigger;
using StudyOrganizer.Settings;

namespace StudyOrganizer.Services.TriggerService.Jobs;

public class CronJobObserverService : IService
{
    private readonly CronJobAggregator _cronJobAggregator;
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;    
    private readonly WorkingPaths _workingPaths;
    private FileSystemWatcher _observer;

    public CronJobObserverService(
        CronJobAggregator cronJobAggregator, 
        PooledDbContextFactory<MyDbContext> dbContextFactory,
        WorkingPaths workingPaths)
    {
        _cronJobAggregator = cronJobAggregator;
        _dbContextFactory = dbContextFactory;
        _workingPaths = workingPaths;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartObserving();
        return Task.CompletedTask;
    }

    private void StartObserving()
    {
        _observer = new FileSystemWatcher(_workingPaths.TriggersSettingsDirectory);
        _observer.NotifyFilter = NotifyFilters.LastWrite;

        _observer.Changed += OnChanged;
        _observer.Error += OnError;

        _observer.Filter = "*.json";
        _observer.EnableRaisingEvents = true;
        Log.Logger.Information($"Запущен мониторинг директории {_workingPaths.TriggersSettingsDirectory}");
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

        using var dbContext = _dbContextFactory.CreateDbContext();
        var triggerInDatabase = dbContext.Triggers.Find(trigger.Name);
        var changes = ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(
            settings,
            trigger.Settings);
        ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(settings,
            triggerInDatabase?.Settings);
        dbContext.SaveChanges();

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