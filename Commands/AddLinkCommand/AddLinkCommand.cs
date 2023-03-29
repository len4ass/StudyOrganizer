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

namespace AddLinkCommand;

public class AddLinkCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public AddLinkCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "addlink";
        Description = "Добавляет ссылку в базу данных по имени, uri и описанию.";
        Format = "/addlink <name> <uri> <optional:description>";
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

        await BotMessager.Reply(
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

        string name = arguments[0];
        string uri = arguments[1];
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

        return await AddLinkToDatabase(
            userInfo, 
            name, 
            uri, 
            description);
    }

    private async Task<BotResponse> AddLinkToDatabase(
        UserInfo userInfo,
        string name, 
        string uri, 
        string description)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var linkRepository = new LinkInfoRepository(dbContext);

        var link = await linkRepository.FindAsync(name);
        if (link is not null)
        {
            return BotResponseFactory.EntryAlreadyExists(
                Name, 
                "ссылка", 
                name, 
                userInfo.Handle!, 
                userInfo.Id);
        }

        await linkRepository.AddAsync(
            new LinkInfo(
                name, 
                description, 
                uri));
        await linkRepository.SaveAsync();
        
        return BotResponseFactory.Success(
            Name, 
            $"Ссылка <b>{name}</b> успешно добавлена в базу данных.", 
            userInfo.Handle!, 
            userInfo.Id);
    }
}