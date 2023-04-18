using System.Text.RegularExpressions;
using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Services.BotService;
using StudyOrganizer.Services.BotService.Command;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Services.OpenAi;
using StudyOrganizer.Services.OpenAi.TextToCommand;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = StudyOrganizer.Services.BotService.Command.BotCommand;

namespace AnalyzeTextCommand;

public sealed class AnalyzeTextCommand : BotCommand
{
    private readonly BotCommandAggregator _botCommandAggregator;
    private readonly IOpenAiTextAnalyzer _openAiTextAnalyzer;

    public AnalyzeTextCommand(BotCommandAggregator botCommandAggregator, IOpenAiTextAnalyzer openAiTextAnalyzer)
    {
        Name = "analyzetext";
        Description = "Анализирует текст и преобразовывает его в команду.";
        Format = "/analyzetext <text>";
        Settings = new CommandSettings
        {
            AccessLevel = AccessLevel.Normal
        };

        _botCommandAggregator = botCommandAggregator;
        _openAiTextAnalyzer = openAiTextAnalyzer;
    }

    public override async Task<UserResponse> ExecuteAsync(
        ITelegramBotClient client,
        Message message,
        UserInfo userInfo,
        IList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            var response = UserResponseFactory.NotEnoughArguments(Name);

            await BotMessager.Reply(
                client,
                message,
                response.Response);
            return response;
        }

        var messageResult = await BotMessager.Reply(
            client,
            message,
            "Начата обработка вашего сообщения.");

        var command = await _openAiTextAnalyzer.TextToCommandAsync(string.Join(' ', arguments));
        if (command.ResponseStatus == OpenAiResponseStatus.Unsupported)
        {
            await BotMessager.EditMessage(
                client,
                messageResult,
                "Обработка текста не доступна.");

            return UserResponseFactory.FailedAnalyzingText(Name);
        }

        if (command.ResponseStatus == OpenAiResponseStatus.Failed ||
            CommandToTokens(command.Result)
                .Count ==
            0)
        {
            await BotMessager.EditMessage(
                client,
                messageResult,
                "Не удалось обработать ваше сообщение.");

            return UserResponseFactory.FailedAnalyzingText(Name);
        }

        var commandTokens = CommandToTokens(command.Result);
        await BotMessager.EditMessage(
            client,
            messageResult,
            $"Результат обработки сообщения в команду: \n<code>/{string.Join(' ', commandTokens)}</code>");

        var userResponse = await _botCommandAggregator.ExecuteCommandByNameAsync(
            commandTokens[0],
            client,
            message,
            userInfo,
            commandTokens.Skip(1)
                .ToList());
        return userResponse.UserResponse;
    }

    private IList<string> CommandToTokens(string command)
    {
        var regex = new Regex("/.*");
        var match = regex.Match(command);
        if (!match.Success)
        {
            return Array.Empty<string>();
        }

        var matchedCommand = match.ToString();
        return TextParser.ParseMessageToCommand(matchedCommand);
    }
}