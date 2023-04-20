using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.Link;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace GetLinksCommand;

public sealed class GetLinksCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public GetLinksCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "getlinks";
        Description = "Получает все ссылки из базы данных.";
        Format = "/getlinks";
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
        if (arguments.Count > 0)
        {
            return UserResponseFactory.ArgumentLimitExceeded(Name);
        }

        var userResponse = await FormatAllLinks();
        return UserResponseFactory.Success(userResponse);
    }

    private string GetLinkString(LinkInfo link, int index)
    {
        return $"<b>{index}</b>. {link}";
    }

    private async Task<string> FormatAllLinks()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var links = await dbContext.Links.ToListAsync();
        if (links.Count == 0)
        {
            return "Ссылок не нашлось.";
        }

        var sb = new StringBuilder("Список всех ссылок: \n\n");
        for (var i = 0; i < links.Count; i++)
        {
            sb.AppendLine(GetLinkString(links[i], i + 1));
        }

        return sb.ToString();
    }
}