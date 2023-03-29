using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace ClearLinksCommand;

public class ClearLinksCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public ClearLinksCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "clearlinks";
        Description = "Удаляет все ссылки из базы данных.";
        Format = "/clearlinks";
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
        if (arguments.Count > 0)
        {
            return BotResponseFactory.ArgumentLimitExceeded(
                Name, 
                userInfo.Handle!, 
                userInfo.Id);
        }
        
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Links");
        return BotResponseFactory.Success(
            Name, 
            "Успешно удалены все ссылки.", 
            userInfo.Handle!, 
            userInfo.Id);
    }
}