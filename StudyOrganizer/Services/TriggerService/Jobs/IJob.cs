namespace StudyOrganizer.Services.TriggerService.Jobs;

public interface IJob
{
    void Start(CancellationToken cancellationToken);
}