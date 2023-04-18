using System.Globalization;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Extensions;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.User;
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
            var parsedDate = DateOnly.TryParse(
                arguments[0],
                new CultureInfo("ru-RU"),
                DateTimeStyles.None,
                out var date);

            if (!parsedDate)
            {
                return UserResponseFactory.FailedParsing(Name);
            }

            var birthdayDate = date.ToDateTime(
                new TimeOnly(
                    0,
                    0,
                    0));
            var birthdayDateUtc = TimeZoneInfo.ConvertTimeToUtc(birthdayDate, _generalSettings.ChatTimeZoneUtc);
            if (birthdayDateUtc > DateTimeOffset.UtcNow)
            {
                return UserResponseFactory.FailedParsing(Name);
            }

            return await UpdateUserBirthdayInDatabase(userInfo, date);
        }

        return UserResponseFactory.ArgumentLimitExceeded(Name);
    }

    private async Task<UserResponse> UpdateUserBirthdayInDatabase(UserInfo userInfo, DateOnly date)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var userRepository = new UserInfoRepository(dbContext);

        var user = await userRepository.FindAsync(userInfo.Id);
        if (user is null)
        {
            return UserResponseFactory.EntryDoesNotExist(
                Name,
                "пользователь с id",
                userInfo.Id.ToString());
        }

        user.Birthday = date;
        await userRepository.SaveAsync();

        var userResponse =
            $"Ваш день рождения успешно установлен на <b>{user.Birthday:dd.MM.yyyy}</b>.";

        return UserResponseFactory.Success(userResponse);
    }
}