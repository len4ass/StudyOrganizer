using System.Text;
using StudyOrganizer.Services.BotService.Handlers.Message;

namespace StudyOrganizer.Services.BotService.Responses;

public class BotResponse
{
    public string? CommandName { get; set; } = default!;
    public IList<string>? CommandArguments { get; set; } = default!;
    public string User { get; set; } = default!;
    public MessageType MessageType { get; set; }
    public UserResponse UserResponse { get; set; } = default!;
    public string InternalResponse { get; set; }

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

    public override string ToString()
    {
        if (MessageType == MessageType.Text)
        {
            return ToStringTextType();
        }

        if (MessageType == MessageType.Voice)
        {
            return ToStringVoiceType();
        }

        return ToStringQueryType();
    }
}