using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Formatters;
using StudyOrganizer.Helper.Reflection;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;
using Exception = System.Exception;

namespace UpdateUserCommand;

public sealed class UpdateUserCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly IValidator<UserDto> _userDtoValidator;

    public UpdateUserCommand(PooledDbContextFactory<MyDbContext> dbContextFactory, IValidator<UserDto> userDtoValidator)
    {
        Name = "updateuser";
        Description = "Обновляет информацию о пользователе.";
        Format = "/updateuser <id/username> <property_1:value_1> ... <property_n:value_n>";
        OtherInfo = "<b>Информация доступная для изменения (property)</b>: \n" +
                    $@"{TextFormatter.BuildHtmlStringFromList(ReflectionHelper.GetPropertyNamesWithTypes(typeof(UserDto)),
                        "\t\t")}";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Owner
        };

        _dbContextFactory = dbContextFactory;
        _userDtoValidator = userDtoValidator;
    }

    public override async Task<UserResponse> ExecuteAsync(
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments)
    {
        var response = await ParseResponse(userInfo, arguments);
        await BotMessager.Reply(
            client,
            message,
            response.Response);

        return response;
    }

    private async Task<UserResponse> ParseResponse(UserInfo userInfo, IList<string> arguments)
    {
        if (arguments.Count < 2)
        {
            return UserResponseFactory.NotEnoughArguments(Name);
        }

        return await UpdateUser(userInfo, arguments);
    }

    private async Task<UserResponse> UpdateUser(UserInfo userInfo, IList<string> arguments)
    {
        var user = arguments[0]
            .Trim('@');

        var isId = long.TryParse(user, out var userId);
        if (isId)
        {
            return await UpdateUserById(
                userInfo,
                arguments.Skip(1)
                    .ToList(),
                userId);
        }

        return await UpdateUserByHandle(
            userInfo,
            arguments.Skip(1)
                .ToList(),
            user);
    }

    private async Task<UserResponse> UpdateUserById(
        UserInfo userInfo,
        IList<string> arguments,
        long id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return UserResponseFactory.UserDoesNotExist(Name, id.ToString());
        }

        var response = UpdateUserData(
            userInfo,
            user,
            arguments);
        await dbContext.SaveChangesAsync();

        return response;
    }

    private async Task<UserResponse> UpdateUserByHandle(
        UserInfo userInfo,
        IList<string> arguments,
        string handle)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Handle == handle);
        if (user is null)
        {
            return UserResponseFactory.UserDoesNotExist(Name, handle);
        }

        var response = UpdateUserData(
            userInfo,
            user,
            arguments);
        await dbContext.SaveChangesAsync();

        return response;
    }

    private UserDto CreateUserDtoFromKeyValuePairs(UserDto previousUserDto, IList<string> arguments)
    {
        var propertyDictionary = arguments.ToDictionary(
            s => s.Split(':')[0],
            s => s.Split(':')[1]);

        return previousUserDto.Adapt<UserDto>()
            .UpdateWithDictionary(propertyDictionary);
    }

    private UserResponse GetFormattedUpdateUserResponse(
        UserInfo userToUpdate,
        UserDto previousUserDto,
        UserDto newUserDto)
    {
        var changes =
            ReflectionHelper.FindPropertyDifferencesBetweenObjectsOfTheSameType(previousUserDto, newUserDto);
        if (changes.Count == 0)
        {
            return UserResponseFactory.Success(
                $"Никаких изменений над пользователем <b>{userToUpdate.Handle ?? userToUpdate.Name}</b> " +
                "не произведено.");
        }

        var userResponse =
            $"Успешно произведены следующие изменения над пользователем <b>{userToUpdate.Handle ?? userToUpdate.Name}</b>: \n\n" +
            $"{TextFormatter.BuildHtmlStringFromList(changes)}";
        return UserResponseFactory.Success(userResponse);
    }

    private UserResponse UpdateUserData(
        UserInfo userInitiator,
        UserInfo userToUpdate,
        IList<string> arguments)
    {
        if (userInitiator.Id == userToUpdate.Id)
        {
            return UserResponseFactory.FailedChangingOwnData(Name);
        }

        var previousUserDto = userToUpdate.Adapt<UserDto>();
        UserDto newUserDto;
        try
        {
            newUserDto = CreateUserDtoFromKeyValuePairs(previousUserDto, arguments);
        }
        catch (Exception e) when (e is IndexOutOfRangeException || e is FormatException)
        {
            return UserResponseFactory.FailedParsing(Name);
        }

        var (isValid, response) = ValidateUserDto(newUserDto);
        if (!isValid)
        {
            return response;
        }

        newUserDto.Adapt(userToUpdate);
        return GetFormattedUpdateUserResponse(
            userToUpdate,
            previousUserDto,
            newUserDto);
    }

    private (bool, UserResponse) ValidateUserDto(UserDto userDto)
    {
        var result = _userDtoValidator.Validate(userDto);
        if (!result.IsValid)
        {
            return (false, UserResponseFactory.FailedParsingSpecified(Name, result.ToString()));
        }

        return (true, default!);
    }
}