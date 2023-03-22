using Serilog;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Hooks;
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

namespace StudyOrganizer;

public class ProgramRunner
{
    private readonly WorkingPaths _workingPaths;
    private readonly IMasterRepository _masterRepository = new MasterRepository();
    
    private GeneralSettings _settings = null!;
    private BotCommandAggregator _commandAggregator = null!;
    private CronJobAggregator _cronJobAggregator = null!;
    private ServiceAggregator _serviceAggregator = null!;
    private ITelegramBotClient _client = null!;

    private readonly MyDbContext _dbCommandContext = new();
    private readonly MyDbContext _dbDeadlineContext = new();
    private readonly MyDbContext _dbLinkContext = new();
    private readonly MyDbContext _dbUserContext = new();
    private readonly MyDbContext _dbTriggerContext = new();

    public ProgramRunner(WorkingPaths workingPaths)
    {
        _workingPaths = workingPaths;
    }

    private void InitializeLogger()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "/data/logs/logs.txt");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(path, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
    
    private void PrepareBot()
    {
        var token = ProgramData.LoadFrom<Token>(_workingPaths.TokenFile);
        ArgumentNullException.ThrowIfNull(token.BotToken);
        _client = new TelegramBotClient(token.BotToken);
    }

    private void LoadSettings()
    {
        _settings = ProgramData.LoadFrom<GeneralSettings>(_workingPaths.SettingsFile);
        ArgumentNullException.ThrowIfNull(_settings);
    }

    private void LoadCommands()
    {
        var commandLoader = new CommandLoader(
            _masterRepository, 
            _settings, 
            _workingPaths.CommandsDirectory,
            _workingPaths.CommandsSettingsDirectory);

        var commands = commandLoader.GetCommandImplementations();
        _commandAggregator = new BotCommandAggregator(commands);
        
        using var dbContext = new MyDbContext();
        dbContext.Commands.Clear();
        foreach (var (_, command) in commands)
        {
            dbContext.Commands.Add(ReflectionHelper.Convert<BotCommand, CommandInfo>(command));
        }

        dbContext.SaveChanges();
    }

    private void LoadCronJobs()
    {
        var cronJobLoader = new CronJobLoader(
                _masterRepository, 
                _settings,
                _client, 
                _workingPaths.TriggersDirectory,
                _workingPaths.TriggersSettingsDirectory);

        var jobs = cronJobLoader.GetTriggerImplementations();
        _cronJobAggregator = new CronJobAggregator(jobs);
        
        using var dbContext = new MyDbContext();
        dbContext.Triggers.Clear();
        foreach (var (_, job) in jobs)
        {
            dbContext.Triggers.Add(
                ReflectionHelper.Convert<SimpleTrigger, TriggerInfo>(job.GetInternalTrigger()));
        }

        dbContext.SaveChanges();
    }
    
    private void InjectRepositories()
    {
        var commandRepository = new CommandInfoRepository(_dbCommandContext);
        var deadlineRepository = new DeadlineInfoRepository(_dbDeadlineContext);
        var linkRepository = new LinkInfoRepository(_dbLinkContext);
        var userRepository = new UserInfoRepository(_dbUserContext);
        var triggerRepository = new SimpleTriggerRepository(_dbUserContext);

        _masterRepository.Add("command", commandRepository);
        _masterRepository.Add("deadline", deadlineRepository);
        _masterRepository.Add("link", linkRepository);
        _masterRepository.Add("user", userRepository);
        _masterRepository.Add("trigger", triggerRepository);
    }

    private IDictionary<string, IService> PrepareServices()
    {
        IDictionary<string, IService> services = new Dictionary<string, IService>();
        /*services.Add("bot", new BotService(
            _masterRepository, 
            _settings, 
            _commandAggregator, 
            _client));
        services.Add("trigger", new SimpleTriggerService(_cronJobAggregator));
        services.Add("bot_command_observer", new BotCommandObserverService(
            _commandAggregator, 
            _dbCommandContext,
            _workingPaths.CommandsSettingsDirectory));
        services.Add("trigger_observer", new CronJobObserverService(
            _cronJobAggregator, 
            _dbTriggerContext,
            _workingPaths.TriggersSettingsDirectory));*/

        return services;
    }

    private async Task StartServices(CancellationToken stoppingToken)
    {
        _serviceAggregator = new ServiceAggregator(PrepareServices());
        await _serviceAggregator.StartAll(stoppingToken);

        Console.ReadLine();
    }

    private void InitializeExitHooks()
    {
        EventHook.AddMethodOnProcessExit((_, _) =>
        {
            Log.Logger.Information("Завершение работы.");
        });
    }

    private void CatchUnhandledExceptions()
    {
        EventHook.AddMethodOnUnhandledException((_, args) =>
        {
            Log.Logger.Error(args.ExceptionObject as Exception, "Необработанное исключение!");
        });
    }

    public async Task Run(CancellationToken stoppingToken)
    {
        InitializeLogger();
        LoadSettings();
        PrepareBot();
        LoadCommands();
        LoadCronJobs();
        InjectRepositories();
        InitializeExitHooks();
        CatchUnhandledExceptions();
        await StartServices(stoppingToken);
    }
}

