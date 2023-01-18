using StudyOrganizer.Enum;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Bot.BotCommand;

namespace HelpCommand;

public sealed class HelpCommand : BotCommand
{
    public HelpCommand(IRepository masterRepository, GeneralSettings generalSettings) 
        : base(masterRepository, generalSettings)
    {
        Name = "help";
        Description = "Выводит список всех команд или описание команды по имени.";
        AccessLevel = AccessLevel.Normal;
    }

    public override Task<string> Execute(
        ITelegramBotClient client, 
        Message message, 
        UserInfo userInfo, 
        IList<string> arguments)
    {
        throw new NotImplementedException();
    }
}