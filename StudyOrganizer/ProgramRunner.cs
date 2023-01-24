using Serilog;
using Serilog.Core;
using StudyOrganizer.Database;
using StudyOrganizer.Hooks;
using StudyOrganizer.Loaders;
using StudyOrganizer.Repositories.Command;
using StudyOrganizer.Repositories.Deadline;
using StudyOrganizer.Repositories.Link;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Repositories.User;
using StudyOrganizer.Services;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;

namespace StudyOrganizer;

public class ProgramRunner
{
    private GeneralSettings _settings = null!;
    
    private MyDbContext _dbContext = new MyDbContext();
    private IMasterRepository _masterRepository = new MasterRepository();
    
    private BotCommandAggregator _commandAggregator = null!;
    private ServiceAggregator _serviceAggregator = null!;

    private ITelegramBotClient _client = null!;

    private void InitializeLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/logs.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
    
    private void LoadSettings()
    {
        ProgramData.AssertSafeFileAccess();
        _settings = ProgramData.LoadFrom<GeneralSettings>(PathContainer.SettingsPath);
        ProgramData.ValidateSettings(_settings);
    }

    private void PrepareBot()
    {
        _client = new TelegramBotClient(_settings.Token!);
    }

    private void LoadCommands()
    {
        var commandLoader = new CommandLoader(
            _masterRepository, 
            _settings, 
            PathContainer.CommandsDirectory);
        
        _commandAggregator = new BotCommandAggregator(commandLoader.GetCommandImplementations());
        _dbContext.Commands.AddRange(commandLoader.GetCommandInfoData());
    }

    private void InjectRepositories()
    {
        var commandRepository = new CommandInfoRepository(_dbContext);
        var deadlineRepository = new DeadlineInfoRepository(_dbContext);
        var linkRepository = new LinkInfoRepository(_dbContext);
        var userRepository = new UserInfoRepository(_dbContext);

        _masterRepository.Add("command", commandRepository);
        _masterRepository.Add("deadline", deadlineRepository);
        _masterRepository.Add("link", linkRepository);
        _masterRepository.Add("user", userRepository);
    }

    private IDictionary<string, IService> PrepareServices()
    {
        IDictionary<string, IService> services = new Dictionary<string, IService>();
        services.Add("bot", new BotService(
            _masterRepository, 
            _settings, 
            _commandAggregator, 
            _client));


        return services;
    }

    private void StartServices()
    {
        _serviceAggregator = new ServiceAggregator(PrepareServices());
        _serviceAggregator.StartAll();

        Console.ReadLine();
    }

    private void InitializeExitHooks()
    {
        EventHook.AddMethodOnProcessExit((_, _) =>
        {
            Log.Logger.Information("Завершение работы.");
            var readOnlyList = (_masterRepository.Find("command") as ICommandInfoRepository)?.GetDataAsync().Result;
            if (readOnlyList is not null)
            {
                _dbContext.Commands.RemoveRange(readOnlyList);
            }
        });
    }

    private void CatchUnhandledExceptions()
    {
        EventHook.AddMethodOnUnhandledException((o, args) =>
        {
            Log.Logger.Error(args.ExceptionObject as Exception, "Необработанное исключение!");
        });
    }

    public void Run()
    {
        InitializeLogger();
        LoadSettings();
        PrepareBot();
        LoadCommands();
        InjectRepositories();
        InitializeExitHooks();
        CatchUnhandledExceptions();
        StartServices();
    }
}

