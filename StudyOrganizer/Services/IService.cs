namespace StudyOrganizer.Services;

public interface IService
{
    Task StartAsync(CancellationToken cancellationToken);
}