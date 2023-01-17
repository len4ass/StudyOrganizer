using StudyOrganizer.Handler.Interface;
using StudyOrganizer.Object;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace StudyOrganizer.Bot;

public class BotService
{
    private IMainHandler _mainHandler;
    private Settings _settings;

    public BotService(IMainHandler mainHandler, Settings settings)
    {
        _mainHandler = mainHandler;
        _settings = settings;
    }

    public void StartService()
    {
        if (_settings.Token is null)
        {
            throw new ArgumentNullException(
                $"Токен не найден. " +
                $"Заполните поле Token в файле settings.json в папке с программой.");
        }
        
        var client = new TelegramBotClient(_settings.Token!);
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

    public async Task PollingUpdateHandler(
        ITelegramBotClient bot, 
        Update update, 
        CancellationToken cts)
    {
        // асинхронно работаем с обновлениями и выбираем более конкретный обработчик для каждого типа
    }
    
    private async Task PollingErrorHandler(
        ITelegramBotClient bot, 
        Exception exception, 
        CancellationToken cts)
    {
        // обрабатываем ошибки
    }
}