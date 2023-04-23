using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace HelpCommand;

public sealed class HelpCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public HelpCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "help";
        Description = "Выводит список всех команд или описание команды по имени.";
        Format = "/help <optional:command_name>";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Normal
        };

        _dbContextFactory = dbContextFactory;
    }

    public override async Task<UserResponse> ExecuteAsync(
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments)
    {
        var response = await ParseResponse(arguments);

        await BotMessager.Reply(
            client,
            message,
            response.Response);

        return response;
    }

    private async Task<UserResponse> ParseResponse(IList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            var userResponse = await FormatAllCommands();
            return UserResponseFactory.Success(userResponse);
        }

        if (arguments.Count == 1)
        {
            var userResponse = await FormatCommand(arguments[0]);
            return UserResponseFactory.Success(userResponse);
        }

        return UserResponseFactory.ArgumentLimitExceeded(Name);
    }

    private async Task<string> FormatAllCommands()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var commands = await dbContext.Commands.ToListAsync();

        var sb = new StringBuilder("Список всех команд: \n \n");
        for (var i = 0; i < commands.Count; i++)
        {
            sb.AppendLine($"<b>{i + 1}</b>. {commands[i]}");
        }

        return sb.ToString();
    }

    private async Task<string> FormatCommand(string name)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var command = await dbContext.Commands.FindAsync(name);
        if (command is null)
        {
            return $"Команды с именем <b>{name}</b> не существует.";
        }

        return $"Информация о команде <b>{command.Name}</b>: \n\n" +
               $"<b>Описание</b>: {command.Description}\n" +
               $"<b>Уровень доступа</b>: {command.Settings.AccessLevel}\n" +
               $"<b>Формат</b>: <code>{command.Format.RemoveAllHtmlCharacters()}</code>\n" +
               command.OtherInfo;
    }
}