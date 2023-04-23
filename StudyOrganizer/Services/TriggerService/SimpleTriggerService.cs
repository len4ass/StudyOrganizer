using Quartz;
using Serilog;
using StudyOrganizer.Services.TriggerService.Jobs;

namespace StudyOrganizer.Services.TriggerService;

public class SimpleTriggerService : IService
{
    private readonly IScheduler _scheduler;

    public SimpleTriggerService(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _scheduler.Start(cancellationToken).ConfigureAwait(true);
        Log.Logger.Information("Сервис триггеров запущен.");
    }
}