﻿using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Repositories.BotCommand;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace HelpCommand;

public sealed class HelpCommand : BotCommand
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    
    public HelpCommand(PooledDbContextFactory<MyDbContext> dbContextFactory)
    {
        Name = "help";
        Description = "Выводит список всех команд или описание команды по имени.";
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
            var userResponse = await FormatAllCommands();
            return new BotResponse(
                userResponse,
                string.Format(
                    InternalResponses.Success,
                    userInfo.Handle,
                    userInfo.Id,
                    Name));
        }

        if (arguments.Count == 1)
        {
            var userResponse = await FormatCommand(arguments[0]);
            return new BotResponse(
                userResponse,
                string.Format(
                    InternalResponses.Success,
                    userInfo.Handle,
                    userInfo.Id,
                    Name));
        }

        return new BotResponse(
            string.Format(Responses.ArgumentLimitExceeded, Name),
            string.Format(InternalResponses.BadRequest,
                userInfo.Handle,
                userInfo.Id,
                Name));
    }
    
    private async Task<string> FormatAllCommands()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var commandRepository = new CommandInfoRepository(dbContext);
        var commands = await commandRepository.GetDataAsync();
        var sb = new StringBuilder("Список всех команд: \n \n");
        for (int i = 0; i < commands.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {commands[i]}");
        }

        return sb.ToString();
    }

    private async Task<string> FormatCommand(string name)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var commandRepository = new CommandInfoRepository(dbContext);
        var command = await commandRepository.FindAsync(name);
        if (command is null)
        {
            return $"Команды с именем {name} не существует.";
        }

        return $"Информация о команде {command.Name}: \n{command.Description}\nУровень доступа: {command.Settings.AccessLevel}";
    }
}