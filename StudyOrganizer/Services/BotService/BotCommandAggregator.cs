using StudyOrganizer.Bot;
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

    public async Task<string> ExecuteCommandByNameAsync(
        string commandName,
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments)
    {
        if (!_commands.ContainsKey(commandName))
        {
            var response = $"Команды /{commandName} не существует, попробуйте заново.";
            await BotMessager.Reply(
                client, 
                message, 
                response);
            return response;
        }

        return await _commands[commandName].ExecuteAsync(
            client,
            message,
            userInfo,
            arguments);
    }
}