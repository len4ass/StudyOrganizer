using Serilog;
using StudyOrganizer.Enum;
using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Repositories.User;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StudyOrganizer.Services.BotService;

public class BotService : IService
{
    private readonly IMasterRepository _masterRepository;
    private readonly GeneralSettings _generalSettings;
    private readonly BotCommandAggregator _botCommandAggregator;
    private readonly ITelegramBotClient _client;
    
    public BotService(
        IMasterRepository masterRepository, 
        GeneralSettings generalSettings, 
        BotCommandAggregator botCommandAggregator,
        ITelegramBotClient client)
    {
        _masterRepository = masterRepository;
        _generalSettings = generalSettings;
        _botCommandAggregator = botCommandAggregator;
        _client = client;
    }

    public void Start()
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };
        
        _client.StartReceiving(
            PollingUpdateHandler, 
            PollingErrorHandler, 
            receiverOptions);
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

        var userFinder = _masterRepository.Find("user") as IUserInfoRepository;
        var user = await userFinder?.FindAsync(message.From.Id)!;
        if (user is null) 
        {
            return $"Пользователя {message.From.FirstName} ({message.From.Id}) нет в белом списке.";
        }

        if (message.Chat.Id != message.From.Id && message.Chat.Id != _generalSettings.MainChatId)
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

        return await _botCommandAggregator.ExecuteCommandByNameAsync(
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
                return;
            }

            if (update.Message.Chat.Id == _generalSettings.MainChatId 
                && update.Message.From is not null
                && !update.Message.From.IsBot)
            {
                var userRepository = _masterRepository.Find("user") as IUserInfoRepository;
                var user = await userRepository?.FindAsync(update.Message.From.Id)!;
                if (user is null)
                {
                    await userRepository.AddAsync(
                        new UserInfo
                        {
                            Id = update.Message.From.Id,
                            Handle = update.Message.From.Username,
                            Name = update.Message.From.FirstName,
                            Level = AccessLevel.Normal,
                            MsgAmount = 1
                        });
                    
                    await userRepository.SaveAsync();
                }
            }

            var response = await MessageUpdateHandler(
                client,
                update.Message, 
                cts);
            Log.Logger.Information(response);
        }
        
        
        // асинхронно работаем с обновлениями и выбираем более конкретный обработчик для каждого типа
    }
    
    private async Task PollingErrorHandler(
        ITelegramBotClient client, 
        Exception exception, 
        CancellationToken cts)
    {
        Log.Logger.Error(exception, "Поймано исключение во время обработки запроса!");
    }
}