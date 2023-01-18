using StudyOrganizer.Bot;
using StudyOrganizer.Loaders;
using StudyOrganizer.Models.Deadline;
using StudyOrganizer.Models.Link;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories;
using StudyOrganizer.Repositories.Command;
using StudyOrganizer.Repositories.Deadline;
using StudyOrganizer.Repositories.Link;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Repositories.User;
using StudyOrganizer.Settings;

namespace StudyOrganizer;

public class ProgramRunner
{
    private void ValidateSettings(Settings.GeneralSettings generalSettings)
    {
        /*var type = typeof(Settings);
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            var fieldType = field.GetType();
            var fieldValue = ()field.GetValue(settings);
            if (field.GetValue()) 
        }*/
        
        if (generalSettings.Token is null)
        {
            throw new ArgumentNullException(
                $"Токен не найден. " +
                $"Заполните поле Token в файле settings.json в папке с программой.");
        }

        if (generalSettings.OwnerId == 0)
        {
            throw new InvalidDataException(
                "Укажите ID владельца бота (OwnerId) в settings.json в папке с программой.");
        }

        if (generalSettings.MainChatId == 0)
        {
            throw new InvalidDataException(
                "Укажите ID основного чата (MainChatId) в settings.json в папке с программой.");
        }

        if (generalSettings.ImportantChatId == 0)
        {
            throw new InvalidDataException(
                $"Укажите ID важного чата (ImportantChatId) в settings.json в папке с программой.");
        }

        if (generalSettings.ChatTimeZoneUtc is null)
        {
            throw new InvalidDataException(
                "Укажите тайм-зону основного чата (ChatTimeZoneUtc) в settings.json в папке с программой.");
        }

        if (generalSettings.HostingTimeZoneUtc is null)
        {
            throw new InvalidDataException(
                "Укажите тайм-зону хостинга (HostingTimeZoneUtc) в settings.json в папке с программой.");
        }
    }
    
    public void Run()
    {
        
        // Сделать приветственную настройку settings.config и добавить изменение
        ProgramData.AssertSafeFileAccess();
        ProgramData.AssertNonEmptyContent();

        // Подгрузка настроек и данных из файлов
        var settings = ProgramData.LoadFrom<GeneralSettings>(PathContainer.SettingsPath);
        var deadlineRepository = new DeadlineInfoRepository(
            ProgramData.LoadFrom<IList<DeadlineInfo>>(PathContainer.DeadlinesPath));
        var linkRepository = new LinkInfoRepository(
            ProgramData.LoadFrom<IList<LinkInfo>>(PathContainer.LinksPath));
        var userRepository = new UserInfoRepository(
            ProgramData.LoadFrom<IList<UserInfo>>(PathContainer.UsersPath));

        // Регистрация хэндлеров
        var mainRepository = new MasterRepository();
        var commandLoader = new CommandLoader(
            mainRepository, 
            settings, 
            PathContainer.DataDirectory);
        
        var commandAggregator = new BotCommandAggregator(commandLoader.GetCommandImplementations());
        var commandRepository = new CommandInfoRepository(commandLoader.GetCommandInfoData());
        mainRepository.Add(new NameRepositoryPair("deadline", deadlineRepository));
        mainRepository.Add(new NameRepositoryPair("link", linkRepository));
        mainRepository.Add(new NameRepositoryPair("user", userRepository));
        mainRepository.Add(new NameRepositoryPair("command", commandRepository));

        var botService = new BotService(mainRepository, settings, commandAggregator);
        botService.StartService();
    }
}

