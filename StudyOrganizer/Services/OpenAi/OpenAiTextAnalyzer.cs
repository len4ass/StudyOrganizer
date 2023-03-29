using System.Text;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using StudyOrganizer.Services.BotService.Command;

namespace StudyOrganizer.Services.OpenAi;

public class OpenAiTextAnalyzer : IOpenAiTextAnalyzer
{
    private readonly OpenAIAPI _openAiApi;
    private readonly BotCommandAggregator _commands;

    private volatile bool _isFirstMessage = true;
    private string _systemMessage = default!;

    public OpenAiTextAnalyzer(OpenAIAPI openAiApi, BotCommandAggregator commands)
    {
        _openAiApi = openAiApi;
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
        
        string request = $"{_systemMessage} \nСообщение: '{text}'";
        var result = await _openAiApi.Chat.CreateChatCompletionAsync(
            new ChatRequest
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0,
                TopP = 1,
                FrequencyPenalty = 0.2,
                PresencePenalty = 0,
                MaxTokens = 50,
                Messages = new ChatMessage[]
                {
                    new ChatMessage(ChatMessageRole.User, request)
                }
            });

        var command = result.ToString();
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