using System.Text;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using StudyOrganizer.Services.BotService.Command;

namespace StudyOrganizer.Services.OpenAi.TextToCommand;

public class OpenAiTextAnalyzer : IOpenAiTextAnalyzer
{
    private readonly OpenAIClient _api;
    private readonly BotCommandAggregator _commands;

    private volatile bool _isFirstMessage = true;
    private string _systemMessage = default!;

    public OpenAiTextAnalyzer(OpenAIClient api, BotCommandAggregator commands)
    {
        _api = api;
        _commands = commands;
    }

    private void CreateSystemMessage()
    {
        var sb = new StringBuilder("Даны команды: \n");
        foreach (var (_, command) in _commands)
        {
            if (command.Name == "analyzetext")
            {
                continue;
            }

            sb.AppendLine(command.Format);
        }

        sb.AppendLine("На сообщение найди корректную команду и преобразуй сообщение в эту команду.");
        sb.AppendLine("Не пиши в ответе ничего кроме команды, отвечай максимально кратко.");
        sb.AppendLine("Если ни одна команда не подходит, то ответь пустой строкой.");
        sb.AppendLine("Не переводи названия дедлайнов, ссылок на английский язык.");
        sb.AppendLine("Форматирование даты: dd.MM.yyyy HH:mm:ss");
        sb.AppendLine($"Учитывай, что текущий год {DateTime.UtcNow.Year}, текущий месяц {DateTime.UtcNow.Month}.");

        _systemMessage = sb.ToString();
    }

    public async Task<OpenAiResponse> TextToCommandAsync(string text)
    {
        if (_isFirstMessage)
        {
            _isFirstMessage = false;
            CreateSystemMessage();
        }

        var request = $"{_systemMessage} \nСообщение: '{text}'";
        var result = await _api.ChatEndpoint.GetCompletionAsync(
            new ChatRequest(
                new[]
                {
                    new ChatPrompt("user", request)
                },
                Model.GPT3_5_Turbo,
                0,
                1,
                frequencyPenalty: 0.2,
                presencePenalty: 0,
                maxTokens: 50));

        var command = result.FirstChoice.ToString();
        if (command.Trim() == string.Empty || command.Trim() == "/")
        {
            return new OpenAiResponse
            {
                ResponseStatus = OpenAiResponseStatus.Failed
            };
        }

        return new OpenAiResponse
        {
            ResponseStatus = OpenAiResponseStatus.Ok,
            Result = command
        };
    }
}