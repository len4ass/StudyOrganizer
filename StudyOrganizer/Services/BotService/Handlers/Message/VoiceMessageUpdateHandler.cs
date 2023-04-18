using System.Text.RegularExpressions;
using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Services.BotService.Command;
using StudyOrganizer.Services.BotService.Responses;
using StudyOrganizer.Services.OpenAi;
using StudyOrganizer.Services.OpenAi.TextToCommand;
using StudyOrganizer.Services.YandexSpeechKit;
using Telegram.Bot;

namespace StudyOrganizer.Services.BotService.Handlers.Message;

public class VoiceMessageUpdateHandler : IUpdateHandler<Telegram.Bot.Types.Message>
{
    private readonly YandexSpeechAnalyzer _yandexSpeechAnalyzer;
    private readonly IOpenAiTextAnalyzer _openAiTextAnalyzer;
    private readonly BotCommandAggregator _botCommandAggregator;

    public VoiceMessageUpdateHandler(
        YandexSpeechAnalyzer yandexSpeechAnalyzer,
        IOpenAiTextAnalyzer openAiTextAnalyzer,
        BotCommandAggregator botCommandAggregator)
    {
        _yandexSpeechAnalyzer = yandexSpeechAnalyzer;
        _openAiTextAnalyzer = openAiTextAnalyzer;
        _botCommandAggregator = botCommandAggregator;
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

    private async Task<BotResponse> HandleTextProccessingAsync(
        ITelegramBotClient client,
        Telegram.Bot.Types.Message message,
        string processedVoiceMessage,
        UserInfo userInfo)
    {
        var messageResult = await BotMessager.Reply(
            client,
            message,
            "Начата обработка вашего сообщения.");

        var command = await _openAiTextAnalyzer.TextToCommandAsync(processedVoiceMessage);
        if (command.ResponseStatus == OpenAiResponseStatus.Unsupported)
        {
            await BotMessager.EditMessage(
                client,
                messageResult,
                "Обработка голосовых сообщений не доступна.");

            return new BotResponse
            {
                User = userInfo.Handle ?? userInfo.Name,
                MessageType = MessageType.Voice,
                InternalResponse = "Обработка голосовых сообщений в команды не доступна."
            };
        }

        if (command.ResponseStatus == OpenAiResponseStatus.Failed ||
            CommandToTokens(command.Result)
                .Count ==
            0)
        {
            await BotMessager.EditMessage(
                client,
                messageResult,
                "Не удалось обработать ваше голосовое сообщение.");

            return new BotResponse
            {
                User = userInfo.Handle ?? userInfo.Name,
                MessageType = MessageType.Voice,
                InternalResponse = "Не удалось обработать голосовое сообщение в команду."
            };
        }

        var commandTokens = CommandToTokens(command.Result);
        await BotMessager.EditMessage(
            client,
            messageResult,
            $"Результат обработки голосового сообщения в команду: \n<code>/{string.Join(' ', commandTokens)}</code>");

        return await _botCommandAggregator.ExecuteCommandByNameAsync(
            commandTokens[0],
            client,
            message,
            userInfo,
            commandTokens.Skip(1)
                .ToList());
    }

    public async Task<BotResponse> HandleAsync(
        ITelegramBotClient client,
        Telegram.Bot.Types.Message update,
        UserInfo user)
    {
        if (update.Voice?.Duration >= 30)
        {
            return new BotResponse
            {
                User = user.Handle ?? user.Name,
                MessageType = MessageType.Voice
            };
        }

        byte[] audioBytes;
        await using (var stream = new MemoryStream())
        {
            await client.GetInfoAndDownloadFileAsync(update.Voice?.FileId!, stream);
            audioBytes = stream.ToArray();
        }

        var response = await _yandexSpeechAnalyzer.SpeechToText(audioBytes, CancellationToken.None);
        if (response.Response == string.Empty)
        {
            return new BotResponse
            {
                User = user.Handle ?? user.Name,
                MessageType = MessageType.Voice
            };
        }

        var text = response.Response;
        if (!text.ToLower()
                .Contains("бот"))
        {
            return new BotResponse
            {
                User = user.Handle ?? user.Name
            };
        }

        text = text.Replace("бот", "")
            .Trim();
        return await HandleTextProccessingAsync(
            client,
            update,
            text,
            user);
    }
}