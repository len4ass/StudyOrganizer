using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StudyOrganizer.Database;
using StudyOrganizer.Loaders;
using StudyOrganizer.Settings;

namespace StudyOrganizer.Services.BotService.Command;

public class BotCommandObserverService : IService
{
    private readonly BotCommandAggregator _botCommandAggregator;
    private readonly MyDbContext _dbContext;
    private readonly string _observedDirectory;
    private FileSystemWatcher _observer;

    public BotCommandObserverService(
        BotCommandAggregator botCommandAggregator, 
        MyDbContext dbContext,
        string observedDirectory)
    {
        _botCommandAggregator = botCommandAggregator;
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
        var fileName = Path.GetFileNameWithoutExtension(path);
        var command = _botCommandAggregator.CommandExists(fileName);
        if (command is null)
        {
            return;
        }

        CommandSettings settings;
        try
        {
            settings = ProgramData.LoadFrom<CommandSettings>(path);
        }
        catch (JsonException e)
        {
            Log.Logger.Error(e, 
                $"Произошла ошибка при попытке получения новых настроек команды {command.Name}!");
            return;
        }
        catch (ArgumentNullException e)
        {
            Log.Logger.Error(e, $"Не удалось обновить триггер {command.Name}!");
            return;
        }

        var commandInDatabase = _dbContext.Commands.Find(command.Name);
        var changes = ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(
            settings,
            command);
        ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(
            settings, 
            commandInDatabase);
        _dbContext.SaveChanges();
        
        foreach (var change in changes)
        {
            Log.Logger.Information(
                $"Изменены настройки команды {command.Name}: " +
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