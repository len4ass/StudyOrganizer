using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OpenAI_API;
using Quartz;
using Serilog;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Loaders;
using StudyOrganizer.Models.Command;
using StudyOrganizer.Models.Trigger;
using StudyOrganizer.Services;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Command;
using StudyOrganizer.Services.OpenAi;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Services.TriggerService.Jobs;
using StudyOrganizer.Services.YandexSpeechKit;
using StudyOrganizer.Settings;
using Telegram.Bot;
using YandexSpeechKitApi;
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

    private void ConfigureDatabasePooling()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseSqlite($"Data Source={_workingPaths.DataBaseFile}")
            .Options;

        var poolingContextFactory = new PooledDbContextFactory<MyDbContext>(options, poolSize: 16);
        _containerBuilder.RegisterInstance(poolingContextFactory).SingleInstance();
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
        var token = ProgramData.LoadFrom<Token>(_workingPaths.BotTokenFile);
        ArgumentNullException.ThrowIfNull(token.Hash);
        _containerBuilder.RegisterInstance(new TelegramBotClient(token.Hash))
            .As<ITelegramBotClient>().SingleInstance();
    }

    private void ConfigureNeuralNetworkApis()
    {
        var openAiToken = ProgramData.LoadFrom<Token>(_workingPaths.OpenApiTokenFile);
        ArgumentNullException.ThrowIfNull(openAiToken.Hash);

        if (openAiToken.Hash != string.Empty)
        {
            var api = new OpenAIAPI(openAiToken.Hash);
            _containerBuilder.RegisterInstance(api)
                .SingleInstance();
            
            _containerBuilder.RegisterInstance(new OpenAiTextAnalyzer(api, _commandAggregator))
                .As<IOpenAiTextAnalyzer>()
                .SingleInstance();
            
            var yandexToken = ProgramData.LoadFrom<Token>(_workingPaths.YandexCloudApiTokenFile);
            ArgumentNullException.ThrowIfNull(yandexToken.Hash);
            if (yandexToken.Hash != string.Empty)
            {
                _containerBuilder.RegisterInstance(new SpeechKitClient(yandexToken.Hash))
                    .As<ISpeechKitClient>()
                    .SingleInstance();
            }
            else
            {
                _containerBuilder.RegisterInstance(new EmptySpeechKitClient())
                    .As<ISpeechKitClient>()
                    .SingleInstance();
            }
        }
        else
        {
            _containerBuilder.RegisterInstance(new EmptyTextAnalyzer())
                .As<IOpenAiTextAnalyzer>()
                .SingleInstance();
            _containerBuilder.RegisterInstance(new EmptySpeechKitClient())
                .As<ISpeechKitClient>()
                .SingleInstance();
        }
    }

    private void ConfigureSpeechToTextApi()
    {
        _containerBuilder.RegisterType<YandexSpeechAnalyzer>().SingleInstance();
    }

    private void ConfigureSettings()
    {
        var settings = ProgramData.LoadFrom<GeneralSettings>(_workingPaths.SettingsFile);
        ArgumentNullException.ThrowIfNull(settings);
        _containerBuilder.RegisterInstance(settings).SingleInstance();
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

    private void RegisterQuartzJobs()
    {
        _containerBuilder.RegisterModule(new QuartzAutofacFactoryModule()); 
        var accordingFiles = Directory.GetFiles(_workingPaths.TriggersDirectory, "*.dll");
        foreach (var assemblyPath in accordingFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            _containerBuilder.RegisterModule(new QuartzAutofacJobsModule(assembly));
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(SimpleTrigger).IsAssignableFrom(type))
                {
                    _loadedTypes.Add(type);
                }
            }
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

        var dbContextFactory = _container.Resolve<PooledDbContextFactory<MyDbContext>>();
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Commands.Clear();
        foreach (var (_, command) in _commandAggregator)
        {
            var commandInfo = ReflectionHelper.Convert<BotCommand, CommandInfo>(command);
            dbContext.Commands.Add(commandInfo);
        }
        
        dbContext.SaveChanges();
    }

    private void LoadCronJobs()
    {
        var scheduler = _container.Resolve<IScheduler>();
        foreach (var type in _loadedTypes) 
        {
            if (type.BaseType != typeof(SimpleTrigger))
            {
                continue;
            }

            if (_container.Resolve(type) is not SimpleTrigger simpleTrigger)
            {
                continue;
            }
            
            Log.Logger.Information($"Загружен триггер {simpleTrigger.Name}.");
            var settingsFile = Path.Combine(_workingPaths.TriggersSettingsDirectory, $"{simpleTrigger.Name}.json");
            if (File.Exists(settingsFile))
            {
                var settings = ProgramData.LoadFrom<TriggerSettings>(settingsFile);
                var changes = ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(
                    settings, 
                    simpleTrigger.Settings);
                foreach (var change in changes)
                {
                    Log.Logger.Information(
                        $"Изменены настройки триггера {simpleTrigger.Name} при загрузке: " +
                        $"значение {change.Name} изменено с {change.PreviousValue} на {change.CurrentValue}");
                }
            }

            var job = JobBuilder.Create(type)
                .WithIdentity(simpleTrigger.Name, "worker_service")
                .StoreDurably()
                .Build();
            var trigger = QuartzExtensions.BuildTrigger(simpleTrigger);
            
            scheduler.ScheduleJob(job, trigger).GetAwaiter().GetResult();
            if (!simpleTrigger.Settings.ShouldRun)
            {
                scheduler.PauseTrigger(trigger.Key).GetAwaiter().GetResult();
            }
            
            _cronJobAggregator.RegisterJob(simpleTrigger.Name, new CronJob
            {
                LoadedTrigger = simpleTrigger,
                JobKey = job.Key,
                TriggerKey = trigger.Key
            });
        }

        var dbContextFactory = _container.Resolve<PooledDbContextFactory<MyDbContext>>();
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Triggers.Clear();
        foreach (var (_, cronJob) in _cronJobAggregator)
        {
            dbContext.Triggers.Add(
                ReflectionHelper.Convert<SimpleTrigger, TriggerInfo>(cronJob.LoadedTrigger));
        }

        dbContext.SaveChanges();
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
        ConfigureNeuralNetworkApis();
        ConfigureSpeechToTextApi();
        ConfigureSettings();
        ConfigureDatabasePooling();
        RegisterDynamicTypes<BotCommand>(_workingPaths.CommandsDirectory);
        RegisterQuartzJobs();
        ConfigureBackgroundServices();
        BuildContainer();
        LoadCommands();
        LoadCronJobs();
        return CreateServices();
    }
}