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

namespace RemoveLinkCommand;

public sealed class RemoveLinkCommand : BotCommand
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
        if (arguments.Count == 0)
        {
            return UserResponseFactory.NotEnoughArguments(Name);
        }

        if (arguments.Count > 1)
        {
            return UserResponseFactory.ArgumentLimitExceeded(Name);
        }

        return await DeleteLinkByName(arguments[0]);
    }

    private async Task<UserResponse> DeleteLinkByName(string linkName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var linkRepository = new LinkInfoRepository(dbContext);

        var link = await linkRepository.FindAsync(linkName);
        if (link is null)
        {
            return UserResponseFactory.EntryDoesNotExist(
                Name,
                "ссылка",
                linkName);
        }

        await linkRepository.RemoveAsync(link);
        await linkRepository.SaveAsync();

        var userResponse = $"Ссылка <b>{link.Name}</b> успешно удалена из базы данных.";
        return UserResponseFactory.Success(userResponse);
    }
}