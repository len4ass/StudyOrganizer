using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OpenAI;
using Quartz;
using Serilog;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Helper.Reflection;
using StudyOrganizer.Models.Command;
using StudyOrganizer.Models.Deadline;
using StudyOrganizer.Models.Link;
using StudyOrganizer.Models.Trigger;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Command;
using StudyOrganizer.Services.BotService.Handlers.Message;
using StudyOrganizer.Services.BotService.Handlers.Query;
using StudyOrganizer.Services.OpenAi.SpeechToText;
using StudyOrganizer.Services.OpenAi.TextToCommand;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Services.TriggerService.Jobs;
using StudyOrganizer.Services.YandexSpeechKit;
using StudyOrganizer.Settings;
using StudyOrganizer.Settings.SimpleTrigger;
using StudyOrganizer.Validators.Command;
using StudyOrganizer.Validators.Deadline;
using StudyOrganizer.Validators.Link;
using StudyOrganizer.Validators.Trigger;
using StudyOrganizer.Validators.User;
using Telegram.Bot;
using YandexSpeechKitApi.Clients;
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

        var poolingContextFactory = new PooledDbContextFactory<MyDbContext>(options, 32);
        _containerBuilder.RegisterInstance(poolingContextFactory)
            .SingleInstance();
    }

    private void ConfigureAggregators()
    {
        _containerBuilder.RegisterInstance(_workingPaths)
            .SingleInstance();
        _commandAggregator = new BotCommandAggregator();
        _containerBuilder.RegisterInstance(_commandAggregator)
            .SingleInstance();

        _cronJobAggregator = new CronJobAggregator();
        _containerBuilder.RegisterInstance(_cronJobAggregator)
            .SingleInstance();
    }

    private void ConfigureLogger()
    {
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, "/logs/logs.txt"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
        Log.Logger = logger;
        _containerBuilder.RegisterInstance(logger)
            .SingleInstance();
    }

    private void ConfigureValidators()
    {
        _containerBuilder.RegisterInstance(new CommandSettingsValidator())
            .As<IValidator<CommandSettings>>()
            .SingleInstance();

        _containerBuilder.RegisterInstance(new TriggerSettingsValidator())
            .As<IValidator<TriggerSettings>>()
            .SingleInstance();

        _containerBuilder.RegisterInstance(new UserDtoValidator())
            .As<IValidator<UserDto>>()
            .SingleInstance();

        _containerBuilder.RegisterInstance(new DeadlineValidator())
            .As<IValidator<DeadlineInfo>>()
            .SingleInstance();

        _containerBuilder.RegisterInstance(new LinkValidator())
            .As<IValidator<LinkInfo>>()
            .SingleInstance();
    }

    private void ConfigureMapper()
    {
        var mapper = new Mapper();
        _containerBuilder.RegisterInstance(mapper)
            .As<IMapper>()
            .SingleInstance();
    }

    private void ConfigureBot()
    {
        var token = Environment.GetEnvironmentVariable("BOT_TOKEN");
        ArgumentNullException.ThrowIfNull(token);
        _containerBuilder.RegisterInstance(new TelegramBotClient(token))
            .As<ITelegramBotClient>()
            .SingleInstance();
    }

    private void ConfigureNeuralNetworkApis()
    {
        var openAiToken = Environment.GetEnvironmentVariable("OPEN_API_TOKEN");
        if (openAiToken is not null)
        {
            var api = new OpenAIClient(openAiToken);
            _containerBuilder.RegisterInstance(api)
                .SingleInstance();

            _containerBuilder.RegisterInstance(new OpenAiTextAnalyzer(api, _commandAggregator))
                .As<IOpenAiTextAnalyzer>()
                .SingleInstance();

            var yandexToken = Environment.GetEnvironmentVariable("YANDEX_CLOUD_TOKEN");
            if (yandexToken is not null)
            {
                _containerBuilder.RegisterInstance(new SpeechKitClient(yandexToken))
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
            _containerBuilder.RegisterInstance(new EmptySpeechAnalyzer())
                .As<IOpenAiSpeechAnalyzer>()
                .SingleInstance();
        }
    }

    private void ConfigureSpeechToTextApi()
    {
        _containerBuilder.RegisterType<YandexSpeechAnalyzer>()
            .SingleInstance();
    }

    private void ConfigureSettings()
    {
        var settings = ProgramData.LoadFrom<GeneralSettings>(_workingPaths.SettingsFile);
        ArgumentNullException.ThrowIfNull(settings);
        _containerBuilder.RegisterInstance(settings)
            .SingleInstance();
    }

    private void RegisterDynamicTypes<T>(string pathToLook)
    {
        var accordingFiles = Directory.GetFiles(pathToLook, "*.dll");
        foreach (var assemblyPath in accordingFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            _containerBuilder.RegisterAssemblyTypes(assembly)
                .Where(
                    type =>
                    {
                        var canBeAssigned = typeof(T).IsAssignableFrom(type);
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
        var accordingFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.dll");
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
        _containerBuilder.RegisterType<TextMessageUpdateHandler>()
            .SingleInstance();
        _containerBuilder.RegisterType<VoiceMessageUpdateHandler>()
            .SingleInstance();
        _containerBuilder.RegisterType<MessageUpdateHandler>()
            .SingleInstance();
        _containerBuilder.RegisterType<CallbackQueryUpdateHandler>()
            .SingleInstance();

        _containerBuilder.RegisterType<BotService>()
            .Named<IService>("bot_service")
            .As<IService>()
            .SingleInstance();
        _containerBuilder.RegisterType<SimpleTriggerService>()
            .Named<IService>("trigger_service")
            .As<IService>()
            .SingleInstance();
    }

    private void BuildContainer()
    {
        _container = _containerBuilder.Build();
    }

    private void LoadCommand(BotCommand botCommand)
    {
        Log.Logger.Information($"Загружена команда {botCommand.Name}.");
        var settingsFile = Path.Combine(_workingPaths.CommandsSettingsDirectory, $"{botCommand.Name}.json");
        if (File.Exists(settingsFile))
        {
            var settings = ProgramData.LoadFrom<CommandSettings>(settingsFile);
            var changes = ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(
                settings,
                botCommand.Settings);
            foreach (var change in changes)
            {
                Log.Logger.Information($"Изменены настройки команды {botCommand.Name} при загрузке: {change}");
            }
        }

        _commandAggregator.RegisterCommand(botCommand.Name, botCommand);
    }

    private void UpdateCommandsInDatabase()
    {
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

            LoadCommand(command);
        }

        UpdateCommandsInDatabase();
    }

    private void LoadCronJob(SimpleTrigger simpleTrigger)
    {
        Log.Logger.Information($"Загружен триггер {simpleTrigger.Name}.");
        var settingsFile = Path.Combine(_workingPaths.TriggersSettingsDirectory, $"{simpleTrigger.Name}.json");
        if (!File.Exists(settingsFile))
        {
            return;
        }

        var triggerValidator = _container.Resolve<IValidator<TriggerSettings>>();
        var settings = ProgramData.LoadFrom<TriggerSettings>(settingsFile);
        var result = triggerValidator.Validate(settings);
        if (!result.IsValid)
        {
            throw new InvalidTriggerSettingsException(simpleTrigger.Name, result.ToString());
        }

        var changes = ReflectionHelper.UpdateObjectInstanceBasedOnOtherTypeValues(
            settings,
            simpleTrigger.Settings);
        foreach (var change in changes)
        {
            Log.Logger.Information($"Изменены настройки триггера {simpleTrigger.Name} при загрузке: {change}");
        }
    }

    private void ScheduleAndRegisterCronJob(
        Type triggerType,
        SimpleTrigger simpleTrigger,
        IScheduler scheduler)
    {
        var job = JobBuilder.Create(triggerType)
            .WithIdentity(simpleTrigger.Name, "worker_service")
            .StoreDurably()
            .Build();
        var trigger = QuartzExtensions.BuildTrigger(simpleTrigger);

        scheduler.ScheduleJob(job, trigger)
            .GetAwaiter()
            .GetResult();
        if (!simpleTrigger.Settings.ShouldRun)
        {
            scheduler.PauseTrigger(trigger.Key)
                .GetAwaiter()
                .GetResult();
        }

        _cronJobAggregator.RegisterJob(
            simpleTrigger.Name,
            new CronJob
            {
                LoadedTrigger = simpleTrigger,
                JobKey = job.Key,
                TriggerKey = trigger.Key
            });
    }

    private void UpdateCronJobsInDatabase()
    {
        var dbContextFactory = _container.Resolve<PooledDbContextFactory<MyDbContext>>();
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Triggers.Clear();
        foreach (var (_, cronJob) in _cronJobAggregator)
        {
            dbContext.Triggers.Add(ReflectionHelper.Convert<SimpleTrigger, TriggerInfo>(cronJob.LoadedTrigger));
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

            LoadCronJob(simpleTrigger);
            ScheduleAndRegisterCronJob(
                type,
                simpleTrigger,
                scheduler);
        }

        UpdateCronJobsInDatabase();
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
        ConfigureValidators();
        ConfigureMapper();
        ConfigureNeuralNetworkApis();
        ConfigureSpeechToTextApi();
        ConfigureSettings();
        ConfigureDatabasePooling();
        RegisterDynamicTypes<BotCommand>(AppContext.BaseDirectory);
        RegisterQuartzJobs();
        ConfigureBackgroundServices();
        BuildContainer();
        LoadCommands();
        LoadCronJobs();
        return CreateServices();
    }
}