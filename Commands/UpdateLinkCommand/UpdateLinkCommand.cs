using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.Link;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Link;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace UpdateLinkCommand;

public class UpdateLinkCommand : BotCommand
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

    public override async Task<BotResponse> ExecuteAsync(
        ITelegramBotClient client, 
        Message message, 
        UserInfo userInfo, 
        IList<string> arguments)
    {
        var response = await ParseResponse(userInfo, arguments);
        await BotMessager.ReplyNoEmbed(
            client,
            message,
            response.UserResponse);

        return response;
    }

    private async Task<BotResponse> ParseResponse(UserInfo userInfo, IList<string> arguments)
    {
        if (arguments.Count < 2)
        {
            return BotResponseFactory.NotEnoughArguments(
                Name,
                userInfo.Handle!,
                userInfo.Id);
        }

        var name = arguments[0];
        var uri = arguments[1];
        if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
        {
            return BotResponseFactory.FailedParsing(
                Name,
                userInfo.Handle!,
                userInfo.Id);
        }

        string description = arguments.Count == 2 
            ? "Нет описания" 
            : string.Join(' ', arguments.Skip(2));

        return await UpdateLinkInDatabase(
            userInfo, 
            name, 
            uri, 
            description);
    }

    private async Task<BotResponse> UpdateLinkInDatabase(
        UserInfo userInfo, 
        string name, 
        string newUri, 
        string newDescription)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var linkRepository = new LinkInfoRepository(dbContext);

        var link = await linkRepository.FindAsync(name);
        if (link is null)
        {
            return BotResponseFactory.EntryDoesNotExist(
                Name, 
                "ссылка", 
                name, 
                userInfo.Handle!, 
                userInfo.Id);
        }

        var previousUri = link.Uri;
        var previousDescription = link.Description;

        link.Uri = newUri;
        link.Description = newDescription;
        await linkRepository.SaveAsync();
        
        return BotResponseFactory.Success(
            Name, 
            $"Ссылка <b>{name}</b> изменена: \n" +
            $"URI изменено с {previousUri} на {newUri}.\n" +
            $"Описание изменено с '{previousDescription}' на '{newDescription}'.", 
            userInfo.Handle!, 
            userInfo.Id);
    }
}