using System.Text;
using StudyOrganizer.Bot;
using StudyOrganizer.Enum;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Command;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Bot.BotCommand;

namespace HelpCommand;

public sealed class HelpCommand : BotCommand
{
    public HelpCommand(IMasterRepository masterRepository, GeneralSettings generalSettings) 
        : base(masterRepository, generalSettings)
    {
        Name = "help";
        Description = "Выводит список всех команд или описание команды по имени.";
        AccessLevel = AccessLevel.Normal;
    }

    public override async Task<string> Execute(
        ITelegramBotClient client, 
        Message message, 
        UserInfo userInfo, 
        IList<string> arguments)
    {
        if (userInfo.Level < AccessLevel)
        {
            return $"Не удалось использовать команду {Name}: у вас недостаточно прав.";
        }

        var response = ParseResponse(arguments);

        await BotMessager.Reply(
            client,
            message,
            response);

        return response;
    }

    private string FormatAllCommands()
    {
        var commandHandler = MasterRepository.Find("command") as ICommandInfoRepository;
        var commands = commandHandler?.GetData();
        if (commands is null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder("Список всех команд: \n \n");
        for (int i = 0; i < commands.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {commands[i]}");
        }

        return sb.ToString();
    }

    private string FormatCommand(string name)
    {
        var commandHandler = MasterRepository.Find("command") as ICommandInfoRepository;
        var command = commandHandler?.Find("help");
        if (command is null)
        {
            return $"Команды с именем {name} не существует.";
        }

        return $"Информация о команде {command.Name}: \n{command.Description}";
    }
    
    
    public string ParseResponse(IList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return FormatAllCommands();
        }

        if (arguments.Count == 1)
        {
            return FormatCommand(arguments[0]);
        }

        return $"Не удалось использовать команду: превышено количество аргументов.";
    }
}