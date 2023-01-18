using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Repositories;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StudyOrganizer.Bot;

public class BotService
{
    private readonly IRepository _masterRepository;
    private readonly GeneralSettings _generalSettings;
    private readonly BotCommandAggregator _botCommandAggregator;

    public BotService(
        IRepository masterRepository, 
        GeneralSettings generalSettings, 
        BotCommandAggregator botCommandAggregator)
    {
        _masterRepository = masterRepository;
        _generalSettings = generalSettings;
        _botCommandAggregator = botCommandAggregator;
    }

    public void StartService()
    {
        if (_generalSettings.Token is null)
        {
            throw new ArgumentNullException(
                $"Токен не найден. " +
                $"Заполните поле Token в файле settings.json в папке с программой.");
        }

        var client = new TelegramBotClient(_generalSettings.Token!);
        using var cancellationToken = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };
        
        client.StartReceiving(
            PollingUpdateHandler, 
            PollingErrorHandler, 
            receiverOptions, 
            cancellationToken.Token);
        
        cancellationToken.Cancel();
    }

    private async Task<string> MessageUpdateHandler(
        ITelegramBotClient client,
        Message message,
        CancellationToken cts)
    {
        if (message.From is null)
        {
            return "Не удалось идентифицировать отправителя.";
        }

        var masterRepository = _masterRepository as IFindable<string, IRepository>;
        var userFinder = masterRepository?.Find("user") as IFindable<long, UserInfo?>;
        var user = userFinder?.Find(message.From.Id);
        if (user is null)
        {
            return $"Пользователя {message.From.FirstName} ({message.From.Id}) нет в белом списке.";
        }

        if (message.Chat.Id != user.Id && message.Chat.Id != _generalSettings.MainChatId)
        {
            return
                $"Сообщение пользователя {user.Name} ({user.Id}) получено не из основного чата/личных сообщений.";
        }

        if (message.Text is null)
        {
            return $"Сообщение от пользователя {user.Name} ({user.Id}) невозможно обработать.";
        }

        var args = TextParser.ParseMessageToCommand(message.Text);
        if (args.Count == 0)
        {
            return $"Сообщение от пользователя {user.Name} ({user.Id}) не содержит команду.";
        }

        return await _botCommandAggregator.ExecuteCommandByName(
            args[0],
            client,
            message,
            user,
            args.Skip(1).ToList());
    }
    
    public async Task PollingUpdateHandler(
        ITelegramBotClient client, 
        Update update, 
        CancellationToken cts)
    {
        if (update.Type == UpdateType.Message)
        {
            if (update.Message is null)
            {
                // произошла внутренняя ошибка
                return;
            }

            var response = await MessageUpdateHandler(
                client,
                update.Message, 
                cts);
            
            // to-do proper logging
            Console.WriteLine(response);
        }
        
        
        // асинхронно работаем с обновлениями и выбираем более конкретный обработчик для каждого типа
    }
    
    private async Task PollingErrorHandler(
        ITelegramBotClient client, 
        Exception exception, 
        CancellationToken cts)
    {
        // обработка ошибок
    }
}