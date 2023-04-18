using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace UpdateLinkCommand;

public sealed class UpdateLinkCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public UpdateLinkCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "updatelink";
        Description = "Обновляет информацию о ссылку в базе данных.";
        Format = "/updatedeadline <name> <uri> <optional:description>";
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
        await BotMessager.ReplyNoEmbed(
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

        var name = arguments[0];
        var uri = arguments[1];
        if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
        {
            return UserResponseFactory.FailedParsing(Name);
        }

        var description = arguments.Count == 2
            ? "Нет описания"
            : string.Join(' ', arguments.Skip(2));

        return await UpdateLinkInDatabase(
            name,
            uri,
            description);
    }

    private async Task<UserResponse> UpdateLinkInDatabase(
        string name,
        string newUri,
        string newDescription)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var link = await dbContext.Links.FindAsync(name);
        if (link is null)
        {
            return UserResponseFactory.EntryDoesNotExist(
                Name,
                "ссылка",
                name);
        }

        var previousUri = link.Uri;
        var previousDescription = link.Description;

        link.Uri = newUri;
        link.Description = newDescription;
        await dbContext.SaveChangesAsync();

        return UserResponseFactory.Success(
            $"Ссылка <b>{name}</b> изменена: \n" +
            $"URI изменено с {previousUri} на {newUri}.\n" +
            $"Описание изменено с '{previousDescription}' на '{newDescription}'.");
    }
}