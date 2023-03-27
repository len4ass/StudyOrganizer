using System.Text;
using OpenAI_API;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using StudyOrganizer.Services.BotService.Command;

namespace StudyOrganizer.Services.OpenAi;

public class OpenAiTextAnalyzer : IOpenAiTextAnalyzer
{
    private readonly IOpenAIAPI _openAiApi;
    private readonly BotCommandAggregator _commands;
    private string _cachedCommandString = "";

    public OpenAiTextAnalyzer(IOpenAIAPI openAiApi, BotCommandAggregator commands)
    {
        _openAiApi = openAiApi;
        _commands = commands;
    }

    private void PrepareCommandString()
    {
        if (_cachedCommandString != string.Empty)
        {
            return;
        }
        
        lock (_cachedCommandString)
        {
            var sb = new StringBuilder("Даны команды: ");
            foreach (var (_, command) in _commands)
            {
                if (command.Name == "analyzetext")
                {
                    continue;
                }
                
                sb.AppendLine(command.Format);
            }

            sb.AppendLine("На сообщение найди корректную команду и преобразуй сообщение в эту команду.");
            sb.AppendLine("Если ни одна команда не подходит, то верни string.Empty.");
            sb.AppendLine("Используй форматирование для аргументов DateTime: dd.MM.yyyy HH:mm:ss");
            sb.AppendLine($"Учитывай, что текущий год {DateTime.UtcNow.Year}, текущий месяц {DateTime.UtcNow.Month}.");
            _cachedCommandString = sb.ToString();
        }
    }
    
    public async Task<OpenAiResponse> TextToCommandAsync(string text)
    {
        PrepareCommandString();
        var sb = new StringBuilder(_cachedCommandString);
        sb.AppendLine($"Сообщение: \'{text}\'");
        var result = await _openAiApi.Completions.CreateCompletionAsync(
            new CompletionRequest(
                sb.ToString(), 
                Model.DavinciText, 
                50, 
                0, 
                top_p: 1, 
                frequencyPenalty: 0.2, 
                presencePenalty: 0));
        
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