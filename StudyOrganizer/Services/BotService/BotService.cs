using Serilog;
using Serilog.Core;
using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Repositories.Master;
using StudyOrganizer.Repositories.User;
using StudyOrganizer.Services.BotService.Command;
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        var bot = await _client.GetMeAsync(cancellationToken);
        StartReceiving(receiverOptions, cancellationToken);
        Log.Logger.Information($"Бот {bot.Username} ({bot.Id}) успешно начал поллинг.");
        await BotMessager.Send(_client, _generalSettings.MainChatId, "Бот запущен!");
    }
    
    private void StartReceiving(ReceiverOptions receiverOptions, CancellationToken cancellationToken)
    {
        var queuedReceiver = new QueuedUpdateReceiver(_client, receiverOptions, PollingErrorHandler);
        Task.Run(async () =>
        {
            await foreach (var update in queuedReceiver.WithCancellation(cancellationToken))
            {
                try
                {
                    await PollingUpdateHandler(_client, update, cancellationToken);
                }
                catch (Exception exсeption)
                {
                    Log.Logger.Error(exсeption, "Поймано исключение во время обработки запроса!");
                }
            }
        }, cancellationToken).ConfigureAwait(false);
    }
    
    private async Task<BotResponse> MessageUpdateHandler(
        ITelegramBotClient client,
        Message message,
        CancellationToken cts)
    {
        if (message.From is null)
        {
            return new BotResponse("", "Не удалось идентифицировать отправителя");
        }

        var userFinder = _masterRepository.Find("user") as IUserInfoRepository;
        var user = await userFinder?.FindAsync(message.From.Id)!;
        if (user is null)
        {
            return new BotResponse("",
                $"Пользователя {message.From.FirstName} ({message.From.Id}) нет в белом списке.");
        }

        if (message.Chat.Id != message.From.Id && message.Chat.Id != _generalSettings.MainChatId)
        {
            return new BotResponse("",
                $"Сообщение пользователя {user.Name} ({user.Id}) получено не из основного чата/личных сообщений.");
        }

        if (message.Text is null)
        {
            return new BotResponse("",
                $"Сообщение от пользователя {user.Name} ({user.Id}) невозможно обработать.");
        }

        var args = TextParser.ParseMessageToCommand(message.Text);
        if (args.Count == 0)
        {
            return new BotResponse("", 
                $"Сообщение от пользователя {user.Name} ({user.Id}) не содержит команду.");
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
        if (update.Type != UpdateType.Message || update.Message is null)
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
                var newUser = new UserInfo
                {
                    Id = update.Message.From.Id,
                    Handle = update.Message.From.Username,
                    Name = update.Message.From.FirstName,
                    Level = update.Message.From.Id == _generalSettings.OwnerId ? AccessLevel.Owner : AccessLevel.Normal,
                    MsgAmount = 1
                };

                await userRepository.AddAsync(newUser);
                await userRepository.SaveAsync();
                Log.Logger.Information(
                    $"Пользователь {newUser.Handle} ({newUser.Id}) добавлен в белый список.");
            }
        }

        var response = await MessageUpdateHandler(
            client,
            update.Message, 
            cts);
        Log.Logger.Information(response.InternalResponse);
    }
    
    private Task PollingErrorHandler(
        Exception exception, 
        CancellationToken cts)
    {
        return Task.CompletedTask;
    }
}