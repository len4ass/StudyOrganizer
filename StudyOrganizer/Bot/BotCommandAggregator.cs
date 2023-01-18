using StudyOrganizer.Models;
using StudyOrganizer.Models.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StudyOrganizer.Bot;

public class BotCommandAggregator
{
    private readonly IDictionary<string, BotCommand> _commandImplementations;

    public BotCommandAggregator(IDictionary<string, BotCommand> commandImplementations)
    {
        _commandImplementations = commandImplementations;
    }

    public async Task<string> ExecuteCommandByName(
        string commandName,
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments)
    {
        if (!_commandImplementations.ContainsKey(commandName))
        {
            return $"Команды /{commandName} не существует, попробуйте заново.";
        }

        return await _commandImplementations[commandName].Execute(
            client,
            message,
            userInfo,
            arguments);
    }
}