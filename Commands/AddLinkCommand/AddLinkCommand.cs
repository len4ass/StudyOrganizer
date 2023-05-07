using FluentValidation;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.Link;
using StudyOrganizer.Models.User;
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
    private readonly IValidator<LinkInfo> _linkValidator;

    public AddLinkCommand(PooledDbContextFactory<MyDbContext> dbContextFactory, IValidator<LinkInfo> linkValidator)
    {
        Name = "addlink";
        Description = "Добавляет ссылку в базу данных по имени, uri и описанию.";
        Format = "/addlink <name> <uri> <optional:description>";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Normal
        };

        _dbContextFactory = dbContextFactory;
        _linkValidator = linkValidator;
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
        var description = arguments.Count == 2
            ? "Нет описания"
            : string.Join(' ', arguments.Skip(2));

        var linkInfo = new LinkInfo(
            name,
            description,
            uri);
        var (isValid, response) = ValidateLink(linkInfo);
        if (!isValid)
        {
            return response;
        }

        return await AddLinkToDatabase(linkInfo);
    }

    private (bool, UserResponse) ValidateLink(LinkInfo linkInfo)
    {
        var result = _linkValidator.Validate(linkInfo);
        if (!result.IsValid)
        {
            return (false, UserResponseFactory.FailedParsingSpecified(Name, result.ToString()));
        }

        return (true, default!);
    }

    private async Task<UserResponse> AddLinkToDatabase(LinkInfo linkInfo)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var link = await dbContext.Links.FindAsync(linkInfo.Name);
        if (link is not null)
        {
            return UserResponseFactory.LinkAlreadyExists(Name, linkInfo.Name);
        }

        await dbContext.Links.AddAsync(linkInfo);
        await dbContext.SaveChangesAsync();
        return UserResponseFactory.Success($"Ссылка <b>{linkInfo.Name}</b> успешно добавлена в базу данных.");
    }
}