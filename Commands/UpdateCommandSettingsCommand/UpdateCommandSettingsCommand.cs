using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Formatters;
using StudyOrganizer.Loaders;
using StudyOrganizer.Models.Command;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Command;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace UpdateCommandSettingsCommand;

public class UpdateCommandSettingsCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly BotCommandAggregator _botCommandAggregator;
    private readonly WorkingPaths _workingPaths;
    private readonly IValidator<CommandSettings> _commandSettingsValidator;

    public UpdateCommandSettingsCommand(
        PooledDbContextFactory<MyDbContext> dbContextFactory,
        BotCommandAggregator botCommandAggregator,
        WorkingPaths workingPaths,
        IValidator<CommandSettings> commandSettingsValidator)
    {
        Name = "updatecommand";
        Description = "Обновляет настройки команды.";
        Format = "/updatecommand <command_name> <property_1:value_1> ... <property_n:value_n>";
        OtherInfo = "<b>Доступные настройки (property)</b>: \n" +
                    $"{TextFormatter.BuildHtmlStringFromList(ReflectionHelper.GetPropertyNamesWithTypes(typeof(CommandSettings)), "\t\t")}";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Owner
        };

        _dbContextFactory = dbContextFactory;
        _botCommandAggregator = botCommandAggregator;
        _workingPaths = workingPaths;
        _commandSettingsValidator = commandSettingsValidator;
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

        return await MapValuesOnCommandSettingsObject(arguments);
    }

    private void ParseAndMapPropertiesOnObject(CommandSettings commandSettings, IList<string> arguments)
    {
        var propertyDictionary = arguments.ToDictionary(
            s => s.Split(':')[0],
            s => s.Split(':')[1]);

        commandSettings.UpdateWithDictionary(propertyDictionary);
    }

    private async Task SaveSettingsToFile(CommandInfo commandInfo)
    {
        var path = Path.Combine(_workingPaths.CommandsSettingsDirectory, $"{commandInfo.Name}.json");
        await ProgramData.SaveToAsync(path, commandInfo.Settings);
    }

    private void UpdateSettingsOnRuntimeCommand(CommandInfo commandInfo)
    {
        var botCommand = _botCommandAggregator.CommandExists(commandInfo.Name);
        botCommand!.Settings = commandInfo.Settings;
    }

    private UserResponse GetFormattedUpdateCommandResponse(CommandSettings previousSettings, CommandInfo commandInfo)
    {
        var changes =
            ReflectionHelper.FindPropertyDifferencesBetweenObjectsOfTheSameType(previousSettings, commandInfo.Settings);
        if (changes.Count == 0)
        {
            return UserResponseFactory.Success(
                $"Никаких изменений над командой <b>{commandInfo.Name}</b> не произведено.");
        }

        var userResponse = $"Успешно произведены следующие изменения над командой <b>{commandInfo.Name}</b>: \n\n" +
                           $"{TextFormatter.BuildHtmlStringFromList(changes)}";
        return UserResponseFactory.Success(userResponse);
    }

    private async Task<UserResponse> MapValuesOnCommandSettingsObject(IList<string> arguments)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var command = await dbContext.Commands.FindAsync(arguments[0]);
        if (command is null)
        {
            return UserResponseFactory.CommandDoesNotExistInDatabase(Name, arguments[0]);
        }

        var previousSettings = command.Settings.Adapt<CommandSettings>();
        ParseAndMapPropertiesOnObject(
            command.Settings,
            arguments.Skip(1)
                .ToList());

        var (isValid, response) = ValidateCommandSettings(command.Settings);
        if (!isValid)
        {
            command.Settings = previousSettings;
            return response;
        }

        UpdateSettingsOnRuntimeCommand(command);
        await SaveSettingsToFile(command);
        await dbContext.SaveChangesAsync();
        return GetFormattedUpdateCommandResponse(previousSettings, command);
    }

    private (bool, UserResponse) ValidateCommandSettings(CommandSettings settings)
    {
        var result = _commandSettingsValidator.Validate(settings);
        if (!result.IsValid)
        {
            return (false, UserResponseFactory.FailedParsingSpecified(Name, result.ToString()));
        }

        return (true, default!);
    }
}