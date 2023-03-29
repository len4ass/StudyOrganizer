using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Link;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace RemoveLinkCommand;

public class RemoveLinkCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public RemoveLinkCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "removelink";
        Description = "Удаляет ссылку из базы данных по ее названию.";
        Format = "/removelink <name>";
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

        return response;    }

    private async Task<BotResponse> ParseResponse(UserInfo userInfo, IList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return BotResponseFactory.NotEnoughArguments(
                Name, 
                userInfo.Handle!, 
                userInfo.Id);
        }

        if (arguments.Count > 1)
        {
            return BotResponseFactory.ArgumentLimitExceeded(
                Name, 
                userInfo.Handle!, 
                userInfo.Id);
        }

        return await DeleteLinkByName(userInfo, arguments[0]);
    }

    private async Task<BotResponse> DeleteLinkByName(UserInfo userInfo, string linkName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var linkRepository = new LinkInfoRepository(dbContext);
        
        var link = await linkRepository.FindAsync(linkName);
        if (link is null)
        {
            return BotResponseFactory.EntryDoesNotExist(
                Name, 
                "ссылка", 
                linkName, 
                userInfo.Handle!, 
                userInfo.Id);
        }

        await linkRepository.RemoveAsync(link);
        await linkRepository.SaveAsync();
        
        var userResponse = $"Ссылка <b>{link.Name}</b> успешно удалена из базы данных.";
        return BotResponseFactory.Success(
            Name, 
            userResponse, 
            userInfo.Handle!, 
            userInfo.Id);
    }
}