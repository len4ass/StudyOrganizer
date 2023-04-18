using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Formatters;
using StudyOrganizer.Loaders;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.SimpleTrigger;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace GetTriggersCommand;

public sealed class GetTriggersCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public GetTriggersCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "gettriggers";
        Description = "Выводит список триггеров или описание указанного триггера.";
        Format = "/gettriggers <optional:trigger_name>";
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
            var userResponse = await FormatAllTriggers();
            return UserResponseFactory.Success(userResponse);
        }

        if (arguments.Count == 1)
        {
            var userResponse = await FormatTrigger(arguments[0]);
            return UserResponseFactory.Success(userResponse);
        }

        return UserResponseFactory.ArgumentLimitExceeded(Name);
    }

    private async Task<string> FormatTrigger(string triggerName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var triggerRepository = new SimpleTriggerRepository(dbContext);

        var trigger = await triggerRepository.FindAsync(triggerName);
        if (trigger is null)
        {
            return $"Триггера с именем <b>{trigger}</b> не существует.";
        }

        var settingString =
            TextFormatter.BuildHtmlStringFromList(
                ReflectionHelper.GetPropertyNamesWithValues(trigger.Settings),
                "\t\t");

        return $"Информация о триггере <b>{trigger.Name}</b>: \n\n" +
               $"<b>Описание</b>: {trigger.Description}\n" +
               $"<b>Настройки</b>: \n{settingString}";
    }

    private async Task<string> FormatAllTriggers()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var triggerRepository = new SimpleTriggerRepository(dbContext);

        var triggers = await triggerRepository.GetDataAsync();
        if (triggers.Count == 0)
        {
            return "Триггеров не нашлось.";
        }

        var sb = new StringBuilder("Список всех триггеров: \n\n");
        for (var i = 0; i < triggers.Count; i++)
        {
            sb.AppendLine($"<b>{i + 1}</b>. {triggers[i]}");
        }

        return sb.ToString();
    }
}