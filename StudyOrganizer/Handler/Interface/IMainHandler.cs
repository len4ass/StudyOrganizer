namespace StudyOrganizer.Handler.Interface;

public interface IMainHandler
{
    public void RegisterHandler(string handlerName, IBaseHandler handler);

    public IBaseHandler? RetrieveHandler(string handlerName);
}