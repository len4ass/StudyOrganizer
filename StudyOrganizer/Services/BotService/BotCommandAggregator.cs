using StudyOrganizer.Models.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.BotCommand;

namespace StudyOrganizer.Services.BotService;

public class BotCommandAggregator
{
    private IDictionary<string, BotCommand> _commands;

    public BotCommandAggregator(IDictionary<string, BotCommand> commands)
    {
        _commands = commands;
    }

    public async Task<BotResponse> ExecuteCommandByNameAsync(
        string commandName,
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments)
    {
        if (!_commands.ContainsKey(commandName))
        {
            var response = string.Format(Responses.CommandDoesNotExist, commandName);
            await BotMessager.Reply(
                client, 
                message, 
                response);
            
            return new BotResponse
            {
                UserResponse = response,
                InternalResponse = string.Format(
                    InternalResponses.CommandDoesNotExist, 
                    userInfo.Handle, 
                    userInfo.Id, 
                    commandName)
            };
        }

        return await _commands[commandName].ExecuteAsync(
            client,
            message,
            userInfo,
            arguments);
    }
}