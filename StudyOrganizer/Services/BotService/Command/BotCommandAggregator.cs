using StudyOrganizer.Models.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Services.BotService.Command;

public class BotCommandAggregator
{
    private readonly IDictionary<string, BotCommand> _commands;

    public BotCommandAggregator(IDictionary<string, BotCommand> commands)
    {
        _commands = commands;
    }

    public BotCommand? CommandExists(string name)
    {
        if (_commands.ContainsKey(name))
        {
            return _commands[name];
        }

        return null;
    }

    public async Task<BotResponse> ExecuteCommandByNameAsync(
        string commandName,
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments)
    {
        var command = CommandExists(commandName);
        if (command is null)
        {
            var responseCommandDoesNotExist = string.Format(Responses.CommandDoesNotExist, commandName);
            await BotMessager.Reply(
                client, 
                message, 
                responseCommandDoesNotExist);

            return new BotResponse(
                responseCommandDoesNotExist,
                string.Format(
                    InternalResponses.CommandDoesNotExist,
                    userInfo.Handle,
                    userInfo.Id,
                    commandName));
        }

        if (userInfo.Level < command.AccessLevel)
        {
            var responseAccessDenied = string.Format(Responses.AccessDenied, command.Name);
            await BotMessager.Reply(
                client, 
                message, 
                responseAccessDenied);

            return new BotResponse(
                responseAccessDenied,
                string.Format(
                    InternalResponses.AccessDenied,
                    userInfo.Handle,
                    userInfo.Id,
                    command.Name));
        }

        return await command.ExecuteAsync(
            client,
            message,
            userInfo,
            arguments);
    }
}