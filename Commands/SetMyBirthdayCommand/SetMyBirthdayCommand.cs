using System.Globalization;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace SetMyBirthdayCommand;

public sealed class SetMyBirthdayCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly GeneralSettings _generalSettings;

    public SetMyBirthdayCommand(PooledDbContextFactory<MyDbContext> dbContextFactory, GeneralSettings generalSettings)
    {
        Name = "setmybirthday";
        Description = "Устанавливает день рождения текущего пользователя на переданную дату.";
        Format = "/setmybirthday <Date>";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Normal
        };

        _dbContextFactory = dbContextFactory;
        _generalSettings = generalSettings;
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
        if (arguments.Count == 0)
        {
            return UserResponseFactory.NotEnoughArguments(Name);
        }

        if (arguments.Count == 1)
        {
            var (isValid, date, response) = ParseBirthday(arguments[0]);
            if (!isValid)
            {
                return response;
            }

            return await UpdateUserBirthdayInDatabase(userInfo, date);
        }

        return UserResponseFactory.ArgumentLimitExceeded(Name);
    }

    private (bool, DateOnly, UserResponse) ParseBirthday(string date)
    {
        var parsedDate = DateOnly.TryParse(
            date,
            new CultureInfo("ru-RU"),
            DateTimeStyles.None,
            out var dateOnly);

        if (!parsedDate)
        {
            return (false, default, UserResponseFactory.FailedParsingSpecified(Name, "Не удалось пропарсить дату."));
        }

        var birthdayDate = dateOnly.ToDateTime(
            new TimeOnly(
                0,
                0,
                0));
        var birthdayDateUtc = TimeZoneInfo.ConvertTimeToUtc(birthdayDate, _generalSettings.ChatTimeZoneUtc);
        if (birthdayDateUtc > DateTimeOffset.UtcNow)
        {
            return (false, default,
                UserResponseFactory.FailedParsingSpecified(Name, "Указанная дата еще не наступила."));
        }

        return (true, dateOnly, default!);
    }

    private async Task<UserResponse> UpdateUserBirthdayInDatabase(UserInfo userInfo, DateOnly date)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = await dbContext.Users.FindAsync(userInfo.Id);
        if (user is null)
        {
            // Может произойти только в том случае, если пользователя удалили из базы данных, пока шел хэндл этого запроса
            return UserResponseFactory.UserDoesNotExist(Name, userInfo.Handle ?? userInfo.Name);
        }

        user.Birthday = date;
        await dbContext.SaveChangesAsync();

        var userResponse =
            $"Ваш день рождения успешно установлен на <b>{user.Birthday:dd.MM.yyyy}</b>.";

        return UserResponseFactory.Success(userResponse);
    }
}