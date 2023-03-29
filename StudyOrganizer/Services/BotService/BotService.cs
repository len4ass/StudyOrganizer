using Microsoft.EntityFrameworkCore.Infrastructure;
using Serilog;
using StudyOrganizer.Database;
using StudyOrganizer.Models.User;
using StudyOrganizer.Parsers;
using StudyOrganizer.Repositories.User;
using StudyOrganizer.Services.BotService.Command;
using StudyOrganizer.Services.OpenAi;
using StudyOrganizer.Services.YandexSpeechKit;
using StudyOrganizer.Settings;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StudyOrganizer.Services.BotService;

public class BotService : IService
{
    private readonly GeneralSettings _generalSettings;
    private readonly PooledDbContextFactory<MyDbContext> _dbContextFactory;
    
    private readonly BotCommandAggregator _botCommandAggregator;
    private readonly ITelegramBotClient _client;

    private readonly IOpenAiTextAnalyzer _openAiTextAnalyzer;
    private readonly YandexSpeechAnalyzer _yandexSpeechAnalyzer;
    
    public BotService(PooledDbContextFactory<MyDbContext> dbContextFactory,
        GeneralSettings generalSettings, 
        BotCommandAggregator botCommandAggregator,
        ITelegramBotClient client,
        IOpenAiTextAnalyzer openAiTextAnalyzer,
        YandexSpeechAnalyzer yandexSpeechAnalyzer)
    {
        _dbContextFactory = dbContextFactory;
        _generalSettings = generalSettings;
        _botCommandAggregator = botCommandAggregator;
        _client = client;
        _openAiTextAnalyzer = openAiTextAnalyzer;
        _yandexSpeechAnalyzer = yandexSpeechAnalyzer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        var bot = await _client.GetMeAsync(cancellationToken);
        await _client.SetMyCommandsAsync(_botCommandAggregator.GetConvertedCommands(), 
            cancellationToken: cancellationToken);
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
                    Task.Run(async () =>
                    {
                        try
                        {
                            await PollingUpdateHandler(_client, update, cancellationToken);
                        }
                        catch (Exception exсeption)
                        {
                            Log.Logger.Error(exсeption, "Поймано исключение во время обработки запроса!");
                            if (update.Message is not null)
                            {
                                await BotMessager.Reply(
                                    _client, 
                                    update.Message, 
                                    "Не удалось обработать ваше сообщение.");
                            }
                        }

                    }, cancellationToken);
                }
                catch (Exception exсeption)
                {
                    Log.Logger.Error(exсeption, "Поймано исключение во время отпуска таски!");
                }
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<BotResponse> VoiceUpdateHandler(
        ITelegramBotClient client,
        Message message,
        UserInfo user,
        CancellationToken cts)
    {
        if (message.Voice?.Duration >= 30)
        {
            return new BotResponse("",
                $"Голосовое сообщение пользователя {user.Handle} ({user.Id}) превышает 30 секунд.");
        }

        byte[] audioBytes;
        await using (var stream = new MemoryStream())
        {
            await client.GetInfoAndDownloadFileAsync(message.Voice?.FileId!, stream, cts);
            audioBytes = stream.ToArray();
        }
        
        var response = await _yandexSpeechAnalyzer.SpeechToText(
            audioBytes, 
            user, 
            cts);
        if (response.UserResponse == string.Empty)
        {
            return response;
        }

        var text = response.UserResponse;
        if (!text.ToLower().Contains("bot") && !text.ToLower().Contains("бот"))
        {
            Log.Logger.Information(text);
            return new BotResponse(
                "",
                $"Речь не содержит обращения к боту в сообщении {user.Handle} ({user.Id}).");
        }

        text = text.Replace("bot", "").Replace("бот", "").Trim();
        Log.Logger.Information(text);
        return await _botCommandAggregator.ExecuteCommandByNameAsync(
            "analyzetext", 
            client, 
            message, 
            user,
            text.Split().ToList());
    }

    private async Task<BotResponse> MessageUpdateHandler(
        ITelegramBotClient client,
        Message message,
        UserInfo user,
        CancellationToken cts)
    {
        var args = TextParser.ParseMessageToCommand(message.Text!);
        if (args.Count == 0)
        {
            return new BotResponse("", 
                $"Сообщение от пользователя {user.Handle} ({user.Id}) не содержит команду.");
        }

        return await _botCommandAggregator.ExecuteCommandByNameAsync(
            args[0],
            client,
            message,
            user,
            args.Skip(1).ToList());
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
        var userInfoRepository = new UserInfoRepository(dbContext);
        var user = await userInfoRepository.FindAsync(telegramUser.Id);
        if (user is null)
        {
            var newUser = new UserInfo
            {
                Id = telegramUser.Id,
                Handle = telegramUser.Username ?? "unknown",
                Name = telegramUser.FirstName,
                Level = telegramUser.Id == _generalSettings.OwnerId ? AccessLevel.Owner : AccessLevel.Normal,
                MsgAmount = 1
            };

            await userInfoRepository.AddAsync(newUser);
            await userInfoRepository.SaveAsync();
            Log.Logger.Information(
                $"Пользователь {newUser.Handle} ({newUser.Id}) добавлен в белый список.");
        }
        else
        {
            user.MsgAmount++;
            await userInfoRepository.SaveAsync();
        }
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

        var message = update.Message;
        if (message.From is null)
        {
            Log.Logger.Information("Не удалось идентифицировать отправителя.");
            return;
        }

        var telegramUser = message.From;
        var telegramChat = message.Chat;
        await UpdateUserData(
            telegramChat,
            telegramUser,
            cts);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var userInfoRepository = new UserInfoRepository(dbContext);
        var user = await userInfoRepository.FindAsync(telegramUser.Id);
        await dbContext.DisposeAsync();
        
        if (user is null)
        {
            Log.Logger.Information(
                $"Пользователя {telegramUser.Username} ({telegramUser.Id}) нет в белом списке.");
            return;
        }
        
        if (telegramChat.Id != telegramUser.Id && telegramChat.Id != _generalSettings.MainChatId)
        {
            Log.Logger.Information(
                $"Сообщение пользователя {user.Handle} ({user.Id}) получено не из основного чата/личных сообщений.");
        }

        BotResponse? botResponse = null;
        if (message.Voice is not null)
        {
            botResponse = await VoiceUpdateHandler(
                client, 
                message,
                user,
                cts);
        } 
        else if (message.Text is not null)
        {
            botResponse = await MessageUpdateHandler(
                client,
                message,
                user,
                cts);
        }
        
        Log.Logger.Information(botResponse?.InternalResponse);
    }
    
    private Task PollingErrorHandler(
        Exception exception, 
        CancellationToken cts)
    {
        return Task.CompletedTask;
    }
}