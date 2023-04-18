using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.Link;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Link;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;
using Message = Telegram.Bot.Types.Message;

namespace AddLinkCommand;

public sealed class AddLinkCommand : BotCommand
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

        var name = arguments[0];
        var uri = arguments[1];
        if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
        {
            return UserResponseFactory.FailedParsing(Name);
        }

        var description = arguments.Count == 2
            ? "Нет описания"
            : string.Join(' ', arguments.Skip(2));

        return await AddLinkToDatabase(
            name,
            uri,
            description);
    }

    private async Task<UserResponse> AddLinkToDatabase(
        string name,
        string uri,
        string description)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var linkRepository = new LinkInfoRepository(dbContext);

        var link = await linkRepository.FindAsync(name);
        if (link is not null)
        {
            return UserResponseFactory.EntryAlreadyExists(
                Name,
                "ссылка",
                name);
        }

        await linkRepository.AddAsync(
            new LinkInfo(
                name,
                description,
                uri));
        await linkRepository.SaveAsync();

        return UserResponseFactory.Success($"Ссылка <b>{name}</b> успешно добавлена в базу данных.");
    }
}