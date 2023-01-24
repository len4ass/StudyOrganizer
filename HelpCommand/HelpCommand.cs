using System.Text;
using StudyOrganizer.Enum;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Command;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.BotCommand;

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

    public override async Task<string> ExecuteAsync(
        ITelegramBotClient client, 
        Message message, 
        UserInfo userInfo, 
        IList<string> arguments)
    {
        if (userInfo.Level < AccessLevel)
        {
            return $"Не удалось использовать команду {Name}: у вас недостаточно прав.";
        }

        var response = await ParseResponse(arguments);

        await BotMessager.Reply(
            client,
            message,
            response);

        return response;
    }

    private async Task<string> FormatAllCommands()
    {
        var commandRepository = MasterRepository.Find("command") as ICommandInfoRepository;
        var commands = await commandRepository?.GetDataAsync()!;

        var sb = new StringBuilder("Список всех команд: \n \n");
        for (int i = 0; i < commands.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {commands[i]}");
        }

        return sb.ToString();
    }

    private async Task<string> FormatCommand(string name)
    {
        var commandHandler = MasterRepository.Find("command") as ICommandInfoRepository;
        var command = await commandHandler?.FindAsync(name)!;
        if (command is null)
        {
            return $"Команды с именем {name} не существует.";
        }

        return $"Информация о команде {command.Name}: \n{command.Description}";
    }
    
    
    public async Task<string> ParseResponse(IList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return await FormatAllCommands();
        }

        if (arguments.Count == 1)
        {
            return await FormatCommand(arguments[0]);
        }

        return $"Не удалось использовать команду: превышено количество аргументов.";
    }
}