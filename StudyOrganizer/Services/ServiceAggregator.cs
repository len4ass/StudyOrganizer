using Serilog;
using Serilog.Core;

namespace StudyOrganizer.Services;

public class ServiceAggregator
{
    private readonly IDictionary<string, IService> _services;

    public ServiceAggregator()
    {
        _services = new Dictionary<string, IService>();
    }
    
    public ServiceAggregator(IDictionary<string, IService> services)
    {
        _services = services;
    }

    public void RegisterService(string name, IService service)
    {
        _services[name] = service;
    }

    public async Task StartAll(CancellationToken stoppingToken)
    {
        foreach (var (_, service) in _services)
        {
            await service.StartAsync(stoppingToken);
        }
    }

    public async Task StopAll(CancellationTokenSource cts)
    {
        cts.Cancel();
        Log.Logger.Information("Завершение приложения через секунду!");
        await Task.Delay(1000);
    } 
}