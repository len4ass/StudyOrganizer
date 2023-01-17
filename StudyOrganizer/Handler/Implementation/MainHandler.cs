using StudyOrganizer.Handler.Interface;

namespace StudyOrganizer.Handler.Implementation;

public class MainHandler : IMainHandler
{
    private readonly IDictionary<string, IBaseHandler> _handlers;

    public MainHandler()
    {
        _handlers = new Dictionary<string, IBaseHandler>();
    }

    public void RegisterHandler(string handlerName, IBaseHandler handler)
    {
        _handlers[handlerName] = handler;
    }

    public IBaseHandler? RetrieveHandler(string handlerName)
    {
        return !_handlers.ContainsKey(handlerName) ? null : _handlers[handlerName];
    }
}