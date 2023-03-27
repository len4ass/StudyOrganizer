using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.Deadline;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace RemoveDeadlineCommand;

public class RemoveDeadlineCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;

    public RemoveDeadlineCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "removedeadline";
        Description = "Удаляет дедлайн из базы данных по его названию.";
        Format = "/removedeadline <name>";
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

        return await DeleteDeadlineByName(userInfo, arguments[0]);
    }

    private async Task<BotResponse> DeleteDeadlineByName(UserInfo userInfo, string deadlineName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deadlineRepository = new DeadlineInfoRepository(dbContext);
        
        var deadline = await deadlineRepository.FindAsync(deadlineName);
        if (deadline is null)
        {
            return BotResponseFactory.EntryDoesNotExist(
                Name, 
                "дедлайн", 
                deadlineName, 
                userInfo.Handle!, 
                userInfo.Id);
        }

        await deadlineRepository.RemoveAsync(deadline);
        await deadlineRepository.SaveAsync();
        
        var userResponse = $"Дедлайн <b>{deadline.Name}</b> успешно удален из базы данных.";
        return BotResponseFactory.Success(
            Name, 
            userResponse, 
            userInfo.Handle!, 
            userInfo.Id);
    }
}