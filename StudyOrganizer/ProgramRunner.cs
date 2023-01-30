using Serilog;
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
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Services.TriggerService.Jobs;
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
        var commandsInfo = commandLoader.GetCommandInfoData();
        if (!_dbContext.Commands.Any())
        {
            _dbContext.Commands.AddRange(commandsInfo);
            _dbContext.SaveChanges();
            return;
        }

        var commands = _dbContext.Commands.ToList();
        foreach (var command in commands)
        {
            if (!commandsInfo.Contains(command))
            {
                _dbContext.Commands.Remove(command);
                _dbContext.SaveChanges();
            }
            else
            {
                _dbContext.Commands.Remove(command);
                _dbContext.SaveChanges();
                _dbContext.Commands.Add(command);
                _dbContext.SaveChanges();
            }
        }
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
        services.Add("trigger", new TriggerService(PrepareCrons()));


        return services;
    }

    private IDictionary<string, IJob> PrepareCrons()
    {
        IDictionary<string, IJob> crons = new Dictionary<string, IJob>();
        crons.Add("test", new CustomJob(new TestTrigger(_masterRepository, _client, _settings)));
        return crons;
    }

    private async Task StartServices()
    {
        _serviceAggregator = new ServiceAggregator(PrepareServices());
        await _serviceAggregator.StartAll();
        

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
        EventHook.AddMethodOnUnhandledException((_, args) =>
        {
            Log.Logger.Error(args.ExceptionObject as Exception, "Необработанное исключение!");
        });
    }

    public async Task Run()
    {
        InitializeLogger();
        LoadSettings();
        PrepareBot();
        LoadCommands();
        InjectRepositories();
        InitializeExitHooks();
        CatchUnhandledExceptions();
        await StartServices();
    }
}

