using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Link;
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

    private async Task<string> FormatAllLinks()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var linkRepository = new LinkInfoRepository(dbContext);

        var links = await linkRepository.GetDataAsync();
        if (links.Count == 0)
        {
            return "Ссылок не нашлось.";
        }

        var sb = new StringBuilder("Список всех ссылок: \n\n");
        for (var i = 0; i < links.Count; i++)
        {
            sb.AppendLine($"<b>{i + 1}</b>. {links[i]}");
        }

        return sb.ToString();
    }
}