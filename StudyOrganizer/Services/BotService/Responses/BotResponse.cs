using System.Text;
using StudyOrganizer.Services.BotService.Handlers.Message;

namespace StudyOrganizer.Services.BotService.Responses;

public class BotResponse
{
    public string? CommandName { get; init; }
    public IList<string>? CommandArguments { get; init; }
    public string User { get; init; } = default!;
    public MessageType MessageType { get; init; }
    public UserResponse UserResponse { get; init; } = default!;
    public string InternalResponse { get; init; } = default!;

    private string ToStringTextType()
    {
        if (CommandName is null || CommandArguments is null)
        {
            return $"Пользователь '{User}' написал сообщение в беседу.";
        }

        var sb = new StringBuilder("[");
        for (var i = 0; i < CommandArguments.Count; i++)
        {
            if (i == CommandArguments.Count - 1)
            {
                sb.Append($"\"{CommandArguments[i]}\"");
            }
            else
            {
                sb.Append($"\"{CommandArguments[i]}\", ");
            }
        }

        sb.Append("]");
        return $"Пользователь '{User}' воспользовался командой '{CommandName}' с аргументами {sb}";
    }

    private string ToStringVoiceType()
    {
        return $"Пользователь '{User}' отправил голосовое сообщение.";
    }

    private string ToStringQueryType()
    {
        return $"Пользователь '{User}' отправил callback query.";
    }

    private string ToStringOtherType()
    {
        return $"Сообщение пользователя {User} содержит неподдерживаемый тип сообщений.";
    }

    public override string ToString()
    {
        return MessageType switch
        {
            MessageType.Text => ToStringTextType(),
            MessageType.Voice => ToStringVoiceType(),
            MessageType.Query => ToStringQueryType(),
            MessageType.Other => ToStringOtherType(),
            _ => "Неизвестный MessageType при получении внутреннего состояния BotResponse"
        };
    }
}