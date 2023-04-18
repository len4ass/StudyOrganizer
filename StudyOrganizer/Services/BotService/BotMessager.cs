using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace StudyOrganizer.Services.BotService;

public static class BotMessager
{
    public static async Task<Message> Send(
        ITelegramBotClient client, 
        long id, 
        string message)
    {
        return await client.SendTextMessageAsync(
            id, 
            message, 
            parseMode: ParseMode.Html);
    }

    public static async Task DeleteMessage(
        ITelegramBotClient client,
        Message message)
    {
        await client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
    }

    public static async Task EditMessage(
        ITelegramBotClient client,
        Message message,
        string newContent)
    {
        await client.EditMessageTextAsync(
            message.Chat.Id,
            message.MessageId,
            newContent,
            parseMode: ParseMode.Html);
    }

    public static async Task<Message> ReplyNoEmbed(
        ITelegramBotClient client,
        Message message,
        string response)
    {
        return await client.SendTextMessageAsync(
            message.Chat.Id,
            response,
            replyToMessageId: message.MessageId,
            parseMode: ParseMode.Html,
            disableWebPagePreview: true);
    }
    
    public static async Task<Message> Reply(
        ITelegramBotClient client, 
        Message message, 
        string response)
    {
        return await client.SendTextMessageAsync(
            message.Chat.Id,
            response,
            replyToMessageId: message.MessageId,
            parseMode: ParseMode.Html); 
    }

    public static async Task<Message> ReplyKeyboardMarkup(
        ITelegramBotClient client,
        Message message,
        string response,
        InlineKeyboardMarkup keyboardMarkup)
    {
        return await client.SendTextMessageAsync(message.Chat.Id,
            response,
            replyToMessageId: message.MessageId,
            parseMode: ParseMode.Html,
            replyMarkup: keyboardMarkup);
    }
}