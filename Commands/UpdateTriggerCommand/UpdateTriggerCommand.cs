using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Quartz;
using StudyOrganizer;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Formatters;
using StudyOrganizer.Loaders;
using StudyOrganizer.Models.Trigger;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Services.TriggerService.Jobs;
using StudyOrganizer.Settings;
using StudyOrganizer.Settings.SimpleTrigger;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace UpdateTriggerCommand;

public sealed class UpdateTriggerCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly CronJobAggregator _cronJobAggregator;
    private readonly WorkingPaths _workingPaths;
    private readonly IScheduler _scheduler;
    private readonly IValidator<TriggerSettings> _triggerSettingsValidator;

    public UpdateTriggerCommand(
        PooledDbContextFactory<MyDbContext> dbContextFactory,
        CronJobAggregator cronJobAggregator,
        WorkingPaths workingPaths,
        IScheduler scheduler,
        IValidator<TriggerSettings> triggerSettingsValidator)
    {
        Name = "updatetrigger";
        Description = "Обновляет настройки триггера и перезапускает его.";
        Format = "/updatetrigger <trigger_name> <property_1:value_1> ... <property_n:value_n>";
        OtherInfo = "<b>Доступные настройки (property)</b>: \n" +
                    $"{TextFormatter.BuildHtmlStringFromList(ReflectionHelper.GetPropertyNamesWithTypes(typeof(TriggerSettings)), "\t\t")}";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Admin
        };

        _workingPaths = workingPaths;
        _cronJobAggregator = cronJobAggregator;
        _scheduler = scheduler;
        _dbContextFactory = dbContextFactory;
        _triggerSettingsValidator = triggerSettingsValidator;
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
        if (arguments.Count < 2)
        {
            return UserResponseFactory.NotEnoughArguments(Name);
        }

        return await MapValuesOnTriggerSettingsObject(arguments);
    }

    private void ParseAndMapPropertiesOnObject(TriggerSettings triggerSettings, IList<string> arguments)
    {
        var propertyDictionary = arguments.ToDictionary(
            s => s.Split(':')[0],
            s => s.Split(':')[1]);

        triggerSettings.UpdateWithDictionary(propertyDictionary);
    }

    private async Task SaveSettingsToFile(TriggerInfo triggerInfo)
    {
        var path = Path.Combine(_workingPaths.TriggersSettingsDirectory, $"{triggerInfo.Name}.json");
        await ProgramData.SaveToAsync(path, triggerInfo.Settings);
    }

    private async Task RescheduleCronJob(TriggerInfo triggerInfo)
    {
        var cronJob = _cronJobAggregator.JobExists(triggerInfo.Name);
        cronJob!.LoadedTrigger.Settings = triggerInfo.Settings;
        var cronTrigger = QuartzExtensions.BuildTrigger(cronJob.LoadedTrigger);
        await _scheduler.RescheduleJob(cronJob.TriggerKey, cronTrigger);

        cronJob.TriggerKey = cronTrigger.Key;
        if (!triggerInfo.Settings.ShouldRun)
        {
            await _scheduler.PauseTrigger(cronJob.TriggerKey);
        }
    }

    private UserResponse GetFormattedUpdateTriggerResponse(
        TriggerSettings previousSettings,
        TriggerInfo triggerInfo)
    {
        var changes =
            ReflectionHelper.FindPropertyDifferencesBetweenObjectsOfTheSameType(previousSettings, triggerInfo.Settings);
        if (changes.Count == 0)
        {
            return UserResponseFactory.Success(
                $"Никаких изменений над триггером <b>{triggerInfo.Name}</b> не произведено.");
        }

        var userResponse = $"Успешно произведены следующие изменения над триггером <b>{triggerInfo.Name}</b>: \n\n" +
                           $"{TextFormatter.BuildHtmlStringFromList(changes)}";
        return UserResponseFactory.Success(userResponse);
    }

    private async Task<UserResponse> MapValuesOnTriggerSettingsObject(IList<string> arguments)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var trigger = await dbContext.Triggers.FindAsync(arguments[0]);
        if (trigger is null)
        {
            return UserResponseFactory.TriggerDoesNotExistInDatabase(Name, arguments[0]);
        }

        var previousSettings = trigger.Settings.Adapt<TriggerSettings>();
        ParseAndMapPropertiesOnObject(
            trigger.Settings,
            arguments.Skip(1)
                .ToList());

        var (isValid, response) = ValidateTrigger(trigger.Settings);
        if (!isValid)
        {
            trigger.Settings = previousSettings;
            return response;
        }

        await SaveSettingsToFile(trigger);
        await RescheduleCronJob(trigger);
        await dbContext.SaveChangesAsync();
        return GetFormattedUpdateTriggerResponse(previousSettings, trigger);
    }

    private (bool, UserResponse) ValidateTrigger(TriggerSettings settings)
    {
        var result = _triggerSettingsValidator.Validate(settings);
        if (!result.IsValid)
        {
            return (false, UserResponseFactory.FailedParsingSpecified(Name, result.ToString()));
        }

        return (true, default!);
    }
}