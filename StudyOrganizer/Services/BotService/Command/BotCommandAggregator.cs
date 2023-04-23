using System.Collections;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService.Responses;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Services.BotService.Command;

public class BotCommandAggregator : IEnumerable<KeyValuePair<string, BotCommand>>
{
    private readonly IDictionary<string, BotCommand> _commands;

    public BotCommandAggregator()
    {
        _commands = new Dictionary<string, BotCommand>();
    }

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

    public IEnumerable<Telegram.Bot.Types.BotCommand> GetConvertedCommands()
    {
        var convertedCommands = new List<Telegram.Bot.Types.BotCommand>();
        foreach (var (name, command) in _commands)
        {
            convertedCommands.Add(
                new Telegram.Bot.Types.BotCommand
                {
                    Command = command.Name,
                    Description = command.Description
                });
        }

        return convertedCommands;
    }

    public void RegisterCommand(string name, BotCommand botCommand)
    {
        _commands[name] = botCommand;
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
            var responseCommandDoesNotExist = UserResponseFactory.CommandDoesNotExist(commandName);
            await BotMessager.Reply(
                client,
                message,
                responseCommandDoesNotExist.Response);

            return new BotResponse
            {
                User = userInfo.Handle ?? userInfo.Id.ToString(),
                CommandName = commandName,
                CommandArguments = arguments,
                UserResponse = responseCommandDoesNotExist
            };
        }

        if (userInfo.Level < command.Settings.AccessLevel)
        {
            var responseAccessDenied = UserResponseFactory.AccessDenied(command.Name);
            await BotMessager.Reply(
                client,
                message,
                responseAccessDenied.Response);

            return new BotResponse
            {
                User = userInfo.Handle ?? userInfo.Id.ToString(),
                CommandName = commandName,
                CommandArguments = arguments,
                UserResponse = responseAccessDenied
            };
        }

        var response = await command.ExecuteAsync(
            client,
            message,
            userInfo,
            arguments);

        return new BotResponse
        {
            User = userInfo.Handle ?? userInfo.Id.ToString(),
            CommandName = commandName,
            CommandArguments = arguments,
            UserResponse = response
        };
    }

    public IEnumerator<KeyValuePair<string, BotCommand>> GetEnumerator()
    {
        return _commands.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}