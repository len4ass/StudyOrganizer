namespace StudyOrganizer.Services.TriggerService.Jobs;

public interface IJob
{
    void Start(CancellationToken cancellationToken);
    void Enable();
    void Disable();
    SimpleTrigger GetInternalTrigger();
}