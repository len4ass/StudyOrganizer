using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StudyOrganizer.Bot;

public static class BotMessager
{
    public static async Task<Message> Reply(ITelegramBotClient client, Message message, string response)
    {
        return await client.SendTextMessageAsync(
            message.Chat.Id,
            response,
            replyToMessageId: message.MessageId,
            parseMode: ParseMode.Html); 
    }
}