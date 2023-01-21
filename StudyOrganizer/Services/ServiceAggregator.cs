namespace StudyOrganizer.Services;

public class ServiceAggregator
{
    private IDictionary<string, IService> _services;
    
    public ServiceAggregator(IDictionary<string, IService> services)
    {
        _services = services;
    }

    public void StartAll()
    {
        foreach (var (name, service) in _services)
        {
            service.Start();
        }
    }
}