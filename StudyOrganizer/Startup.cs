using System.Reflection;
using Autofac;
using Serilog;
using StudyOrganizer.Database;
using StudyOrganizer.Loaders;
using StudyOrganizer.Models.Command;
using StudyOrganizer.Models.Trigger;
using StudyOrganizer.Repositories.BotCommand;
using StudyOrganizer.Repositories.Deadline;
using StudyOrganizer.Repositories.Link;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Repositories.SimpleTrigger;
using StudyOrganizer.Repositories.User;
using StudyOrganizer.Services;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Command;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Services.TriggerService.Jobs;
using StudyOrganizer.Settings;
using Telegram.Bot;
using IContainer = Autofac.IContainer;

namespace StudyOrganizer;

public class Startup
{
    private readonly WorkingPaths _workingPaths;
    private readonly ContainerBuilder _containerBuilder;
    private readonly IList<Type> _loadedTypes;

    private IContainer _container = null!;
    private BotCommandAggregator _commandAggregator = null!;
    private CronJobAggregator _cronJobAggregator = null!;
    
    public Startup(WorkingPaths workingPaths, ContainerBuilder containerBuilder)
    {
        _workingPaths = workingPaths;
        _containerBuilder = containerBuilder;
        _loadedTypes = new List<Type>();
    }

    private void ConfigureAggregators()
    {
        _containerBuilder.RegisterInstance(_workingPaths).SingleInstance();
        _commandAggregator = new BotCommandAggregator();
        _containerBuilder.RegisterInstance(_commandAggregator).SingleInstance();
        
        _cronJobAggregator = new CronJobAggregator();
        _containerBuilder.RegisterInstance(_cronJobAggregator).SingleInstance();
    }
    
    private void ConfigureLogger()
    {
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "/logs/logs.txt"), 
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
        Log.Logger = logger;
        _containerBuilder.RegisterInstance(logger).SingleInstance();
    }

    private void ConfigureBot()
    {
        var token = ProgramData.LoadFrom<Token>(_workingPaths.TokenFile);
        ArgumentNullException.ThrowIfNull(token.BotToken);
        _containerBuilder.RegisterInstance(new TelegramBotClient(token.BotToken))
            .As<ITelegramBotClient>().SingleInstance();
    }

    private void ConfigureSettings()
    {
        var settings = ProgramData.LoadFrom<GeneralSettings>(_workingPaths.SettingsFile);
        ArgumentNullException.ThrowIfNull(settings);
        _containerBuilder.RegisterInstance(settings).SingleInstance();
    }

    private void ConfigureRepositories()
    {
        _containerBuilder.RegisterType<MasterRepository>().As<IMasterRepository>();
        _containerBuilder.RegisterType<MyDbContext>();
        _containerBuilder.RegisterType<UserInfoRepository>().As<IUserInfoRepository>().SingleInstance();
        _containerBuilder.RegisterType<DeadlineInfoRepository>().As<IDeadlineInfoRepository>().SingleInstance();
        _containerBuilder.RegisterType<LinkInfoRepository>().As<ILinkInfoRepository>().SingleInstance();
        _containerBuilder.RegisterType<CommandInfoRepository>().As<ICommandInfoRepository>().SingleInstance();
        _containerBuilder.RegisterType<SimpleTriggerRepository>().As<ISimpleTriggerRepository>().SingleInstance();
    }

    private void RegisterDynamicTypes<T>(string pathToLook)
    {
        var accordingFiles = Directory.GetFiles(pathToLook, "*.dll");
        foreach (var assemblyPath in accordingFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            _containerBuilder.RegisterAssemblyTypes(assembly)
                .Where(type =>
                {
                    bool canBeAssigned = typeof(T).IsAssignableFrom(type);
                    if (canBeAssigned)
                    {
                        _loadedTypes.Add(type);
                    }
                    
                    return canBeAssigned;
                })
                .AsSelf();
        }
    }

    private void ConfigureBackgroundServices()
    {
        _containerBuilder.RegisterType<BotService>()
            .Named<IService>("bot_service").As<IService>().SingleInstance();
        _containerBuilder.RegisterType<SimpleTriggerService>()
            .Named<IService>("trigger_service").As<IService>().SingleInstance();
        _containerBuilder.RegisterType<BotCommandObserverService>()
            .Named<IService>("bot_command_observer_service").As<IService>().SingleInstance();
        _containerBuilder.RegisterType<CronJobObserverService>()
            .Named<IService>("trigger_observer_service").As<IService>().SingleInstance();
    }

    private void BuildContainer()
    {
        _container = _containerBuilder.Build();
    }

    private void LoadCommands()
    {
        foreach (var type in _loadedTypes) 
        {
            if (type.BaseType != typeof(BotCommand))
            {
                continue;
            }
            
            if (_container.Resolve(type) is not BotCommand command)
            {
                continue;
            }

            Log.Logger.Information($"Загружена команда {command.Name}.");
            var settingsFile = Path.Combine(_workingPaths.CommandsSettingsDirectory, $"{command.Name}.json");
            if (File.Exists(settingsFile))
            {
                var settings = ProgramData.LoadFrom<CommandSettings>(settingsFile);
                var changes = ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(
                    settings, 
                    command.Settings);
                foreach (var change in changes)
                {
                    Log.Logger.Information(
                        $"Изменены настройки команды {command.Name} при загрузке: " +
                        $"значение {change.Name} изменено с {change.PreviousValue} на {change.CurrentValue}");
                }
            }

            _commandAggregator.RegisterCommand(command.Name, command);
        }

        var commandInfoRepository = _container.Resolve<ICommandInfoRepository>();
        commandInfoRepository.ClearAllAsync();
        foreach (var (_, command) in _commandAggregator)
        {
            commandInfoRepository.AddAsync(
                ReflectionHelper.Convert<BotCommand, CommandInfo>(command));
        }

        commandInfoRepository.SaveAsync();
    }

    private void LoadCronJobs()
    {
        foreach (var type in _loadedTypes) 
        {
            if (type.BaseType != typeof(SimpleTrigger))
            {
                continue;
            }
            
            if (_container.Resolve(type) is not SimpleTrigger trigger)
            {
                continue;
            }

            Log.Logger.Information($"Загружен триггер {trigger.Name}.");
            var settingsFile = Path.Combine(_workingPaths.TriggersSettingsDirectory, $"{trigger.Name}.json");
            if (File.Exists(settingsFile))
            {
                var settings = ProgramData.LoadFrom<TriggerSettings>(settingsFile);
                var changes = ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(
                    settings, 
                    trigger.Settings);
                foreach (var change in changes)
                {
                    Log.Logger.Information(
                        $"Изменены настройки триггера {trigger.Name} при загрузке: " +
                        $"значение {change.Name} изменено с {change.PreviousValue} на {change.CurrentValue}");
                }
            }

            _cronJobAggregator.RegisterJob(trigger.Name, new CronJob(trigger));
        }
        
        var triggerRepository = _container.Resolve<ISimpleTriggerRepository>();
        triggerRepository.ClearAllAsync();
        foreach (var (_, job) in _cronJobAggregator)
        {
            triggerRepository.AddAsync(
                ReflectionHelper.Convert<SimpleTrigger, TriggerInfo>(job.GetInternalTrigger()));
        }

        triggerRepository.SaveAsync();
    }

    private IEnumerable<IService> CreateServices()
    {
        return _container.Resolve<IEnumerable<IService>>();
    }

    public IEnumerable<IService> ConfigureServices()
    {
        ConfigureAggregators();
        ConfigureLogger();
        ConfigureBot();
        ConfigureSettings();
        ConfigureRepositories();
        RegisterDynamicTypes<BotCommand>(_workingPaths.CommandsDirectory);
        RegisterDynamicTypes<SimpleTrigger>(_workingPaths.TriggersDirectory);
        ConfigureBackgroundServices();
        BuildContainer();
        LoadCommands();
        LoadCronJobs();
        return CreateServices();
    }
}