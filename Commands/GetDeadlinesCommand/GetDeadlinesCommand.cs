using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.Deadline;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace GetDeadlinesCommand;

public sealed class GetDeadlinesCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly GeneralSettings _settings;

    public GetDeadlinesCommand(PooledDbContextFactory<MyDbContext> dbContextFactory, GeneralSettings settings)
    {
        Name = "getdeadlines";
        Description = "Получает все актуальные дедлайны из базы данных.";
        Format = "/getdeadlines";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Normal
        };

        _dbContextFactory = dbContextFactory;
        _settings = settings;
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
        if (arguments.Count > 0)
        {
            return UserResponseFactory.ArgumentLimitExceeded(Name);
        }

        var validDeadlines = await ExtractValidDeadlines();
        var deadlinesString = BuildDeadlineString(validDeadlines);
        return UserResponseFactory.Success(deadlinesString);
    }

    private async Task<IList<DeadlineInfo>> ExtractValidDeadlines()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        const string queryForValidDeadlinesOrderedByDate = @"SELECT * FROM Deadlines
            WHERE (strftime('%s', DateUtc) >= strftime('%s', 'now'))
            ORDER BY (strftime('%s', DateUtc))";

        var validDeadlines = await dbContext.Deadlines
            .FromSqlRaw(queryForValidDeadlinesOrderedByDate)
            .ToListAsync();
        return validDeadlines;
    }

    private string GetDeadlineString(DeadlineInfo deadline, int index)
    {
        return $"<b>{index}</b>. {deadline.ToString(_settings.ChatTimeZoneUtc)}";
    }

    private string BuildDeadlineString(IList<DeadlineInfo> deadlines)
    {
        if (deadlines.Count == 0)
        {
            return "Дедлайнов нет :)";
        }

        var sb = new StringBuilder("Список текущих дедлайнов: \n\n");
        for (var i = 0; i < deadlines.Count; i++)
        {
            sb.AppendLine(GetDeadlineString(deadlines[i], i + 1));
        }

        return sb.ToString();
    }
}