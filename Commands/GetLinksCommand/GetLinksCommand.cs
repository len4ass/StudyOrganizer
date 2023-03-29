using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Link;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace GetLinksCommand;

public class GetLinksCommand : BotCommand
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
        if (arguments.Count > 0)
        {
            return BotResponseFactory.ArgumentLimitExceeded(
                Name, 
                userInfo.Handle!, 
                userInfo.Id);
        }

        var userResponse = await FormatAllLinks();
        return BotResponseFactory.Success(
            Name, 
            userResponse, 
            userInfo.Handle!, 
            userInfo.Id);
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
        int index = 1;
        foreach (var link in links)
        {
            sb.AppendLine($"<b>{index++}</b>. {link}");
        }

        return sb.ToString();
    }
}