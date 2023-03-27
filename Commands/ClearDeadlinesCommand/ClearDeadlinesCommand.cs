using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace ClearDeadlinesCommand;

public class ClearDeadlinesCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public ClearDeadlinesCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "cleardeadlines";
        Description = "Удаляет все дедлайны из базы данных";
        Format = "/cleardeadlines";
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
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Deadlines");
        return BotResponseFactory.Success(
            Name, 
            "Успешно удалены все дедлайны.", 
            userInfo.Handle!, 
            userInfo.Id);
    }
}