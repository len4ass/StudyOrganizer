using System.Text;
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

    public override async Task<BotResponse> ExecuteAsync(
        ITelegramBotClient client, 
        Message message, 
        UserInfo userInfo, 
        IList<string> arguments)
    {
        var response = await ParseResponse(userInfo, arguments);

        await BotMessager.Reply(
            client,
            message,
            response.UserResponse);

        return response;
    }

    private async Task<BotResponse> ParseResponse(UserInfo userInfo, IList<string> arguments)
    {
        if (userInfo.Level < AccessLevel)
        {
            return new BotResponse
            {
                UserResponse = string.Format(Responses.AccessDenied, Name),
                InternalResponse = string.Format(
                    InternalResponses.AccessDenied,
                    userInfo.Handle, 
                    userInfo.Id, 
                    Name)
            };
        }
        
        if (arguments.Count == 0)
        {
            var userResponse = await FormatAllCommands();
            return new BotResponse
            {
                UserResponse = userResponse,
                InternalResponse = string.Format(
                    InternalResponses.Success, 
                    userInfo.Handle, 
                    userInfo.Id, 
                    Name)
            };
        }

        if (arguments.Count == 1)
        {
            var userResponse = await FormatCommand(arguments[0]);
            return new BotResponse
            {
                UserResponse = userResponse,
                InternalResponse = string.Format(
                    InternalResponses.Success, 
                    userInfo.Handle, 
                    userInfo.Id, 
                    Name)
            };
        }

        return new BotResponse
        {
            UserResponse = string.Format(Responses.ArgumentLimitExceeded, Name),
            InternalResponse = string.Format(InternalResponses.BadRequest, 
                userInfo.Handle, 
                userInfo.Id, 
                Name)
        };
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
}