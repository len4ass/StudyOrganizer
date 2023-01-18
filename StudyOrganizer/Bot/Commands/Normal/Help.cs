using System.Text;
using StudyOrganizer.Enum;
using StudyOrganizer.Models;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Bot.Commands.Normal;

/*public class Help : BotCommand
{
    public Help(IRepository masterRepository, Settings.GeneralSettings generalSettings) 
        : base(masterRepository, generalSettings)
    {
    }

    public override async Task<string> Execute(
        ITelegramBotClient client, 
        Message message, 
        UserInfo userInfo, 
        IList<string> arguments)
    {
        if (user.Level)
        {
            return $"Не удалось использовать команду {Name}: у вас недостаточно прав.";
        }

        var response = ParseResponse(
            message, 
            userInfo, 
            arguments);

        await BotMessager.Reply(
            client,
            message,
            response);

        return response;
    }

    private string FormatAllCommands()
    {
        var commandHandler = MasterRepository.RetrieveRepositoryInterface("command") as ICommandInfoHandler;
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
        var commandHandler = MasterRepository.RetrieveRepositoryInterface("command") as ICommandInfoHandler;
        var command = commandHandler?.Find("help");
        if (command is null)
        {
            return $"Команды с именем {name} не существует.";
        }

        return $"Информация о команде {command.Name}: \n{command.Description}";
    }
    
    
    public string ParseResponse(Message message, UserInfo userInfo, IList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return FormatAllCommands();
        }

        if (arguments.Count == 1)
        {
            return FormatCommand(arguments[0]);
        }

        return $"Не удалось использовать команду : превышено количество аргументов.";
    }
}*/