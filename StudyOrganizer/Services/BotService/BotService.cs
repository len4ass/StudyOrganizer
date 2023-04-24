using Microsoft.EntityFrameworkCore.Infrastructure;
using Serilog;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService.Command;
using StudyOrganizer.Services.BotService.Handlers.Message;
using StudyOrganizer.Services.BotService.Handlers.Query;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StudyOrganizer.Services.BotService;

public class BotService : IService
{
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly MessageUpdateHandler _messageUpdateHandler;
    private readonly CallbackQueryUpdateHandler _callbackQueryUpdateHandler;
    private readonly BotCommandAggregator _botCommandAggregator;
    private readonly GeneralSettings _generalSettings;
    private readonly ITelegramBotClient _client;

    public BotService(
        PooledDbContextFactory<MyDbContext> dbContextFactory,
        MessageUpdateHandler messageUpdateHandler,
        CallbackQueryUpdateHandler callbackQueryUpdateHandler,
        BotCommandAggregator botCommandAggregator,
        GeneralSettings generalSettings,
        ITelegramBotClient client)
    {
        _dbContextFactory = dbContextFactory;
        _messageUpdateHandler = messageUpdateHandler;
        _callbackQueryUpdateHandler = callbackQueryUpdateHandler;
        _botCommandAggregator = botCommandAggregator;
        _generalSettings = generalSettings;
        _client = client;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        var bot = await _client.GetMeAsync(cancellationToken);
        await _client.SetMyCommandsAsync(
            _botCommandAggregator.GetConvertedCommands(),
            cancellationToken: cancellationToken);
        StartReceiving(receiverOptions, cancellationToken);

        Log.Logger.Information($"Бот {bot.Username} ({bot.Id}) успешно начал поллинг.");
        await BotMessager.Send(
            _client,
            _generalSettings.OwnerId,
            "Бот запущен!");
    }

    private void StartReceiving(ReceiverOptions receiverOptions, CancellationToken cancellationToken)
    {
        var queuedReceiver = new QueuedUpdateReceiver(
            _client,
            receiverOptions,
            PollingErrorHandler);

        Task.Run(
                async () =>
                {
                    await foreach (var update in queuedReceiver.WithCancellation(cancellationToken))
                    {
                        try
                        {
                            Task.Run(
                                async () =>
                                {
                                    try
                                    {
                                        await PollingUpdateHandler(
                                            _client,
                                            update,
                                            cancellationToken);
                                    }
                                    catch (Exception exсeption)
                                    {
                                        Log.Logger.Error(
                                            exсeption,
                                            "Поймано исключение во время обработки запроса!");

                                        if (update.Message is not null)
                                        {
                                            await BotMessager.Reply(
                                                _client,
                                                update.Message,
                                                "Не удалось обработать ваше сообщение.");
                                        }
                                    }
                                },
                                cancellationToken);
                        }
                        catch (Exception exсeption)
                        {
                            Log.Logger.Error(exсeption, "Поймано исключение во время отпуска таски!");
                        }
                    }
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task UpdateUserData(
        Chat telegramChat,
        User telegramUser,
        CancellationToken cts)
    {
        if (telegramUser.IsBot || telegramChat.Id != _generalSettings.MainChatId)
        {
            return;
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cts);
        var user = await dbContext.Users.FindAsync(telegramUser.Id);
        if (user is not null)
        {
            user.Handle = telegramUser.Username;
            user.Name = telegramUser.FirstName;
            user.MsgAmount++;
            await dbContext.SaveChangesAsync(cts);
            return;
        }

        user = new UserInfo
        {
            Id = telegramUser.Id,
            Handle = telegramUser.Username,
            Name = telegramUser.FirstName,
            Level = telegramUser.Id == _generalSettings.OwnerId ? AccessLevel.Owner : AccessLevel.Normal,
            MsgAmount = 1
        };

        await dbContext.Users.AddAsync(user, cts);
        await dbContext.SaveChangesAsync(cts);
        Log.Logger.Information($"Пользователь {user.Handle} ({user.Id}) добавлен в белый список.");
    }

    private async Task HandleMessageUpdate(
        ITelegramBotClient client,
        Message message,
        CancellationToken cts)
    {
        if (message.From is null)
        {
            return;
        }

        await UpdateUserData(
            message.Chat,
            message.From,
            cts);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cts);
        var userInfo = await dbContext.Users.FindAsync(message.From!.Id);
        if (userInfo is null)
        {
            Log.Logger.Information(
                $"Пользователя {message.From.Username} ({message.From.Id}) " +
                "нет в белом списке.");
            return;
        }

        if (message.Chat.Id != message.From.Id && message.Chat.Id != _generalSettings.MainChatId)
        {
            Log.Logger.Information(
                $"Сообщение пользователя {userInfo.Handle} ({userInfo.Id}) " +
                "получено не из основного чата/личных сообщений.");
            return;
        }

        var response = await _messageUpdateHandler.HandleAsync(
            client,
            message,
            userInfo);
        Log.Logger.Information(response.ToString());
    }

    private async Task HandleCallbackQueryUpdate(
        ITelegramBotClient client,
        CallbackQuery callbackQuery,
        CancellationToken cts)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cts);
        var userInfo = await dbContext.Users.FindAsync(callbackQuery.From.Id);
        if (userInfo is null)
        {
            Log.Logger.Information(
                $"Пользователя {callbackQuery.From.Username} ({callbackQuery.From.Id}) " +
                "нет в белом списке.");
            return;
        }

        var response = await _callbackQueryUpdateHandler.HandleAsync(
            client,
            callbackQuery,
            userInfo);
        Log.Logger.Information(response.ToString());
    }

    private async Task PollingUpdateHandler(
        ITelegramBotClient client,
        Update update,
        CancellationToken cts)
    {
        if (update.Type == UpdateType.Message)
        {
            await HandleMessageUpdate(
                client,
                update.Message!,
                cts);
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            await HandleCallbackQueryUpdate(
                client,
                update.CallbackQuery!,
                cts);
        }
    }

    private Task PollingErrorHandler(Exception exception, CancellationToken cts)
    {
        return Task.CompletedTask;
    }
}