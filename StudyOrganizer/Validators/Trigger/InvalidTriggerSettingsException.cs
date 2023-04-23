namespace StudyOrganizer.Validators.Trigger;

public class InvalidTriggerSettingsException : Exception
{
    public InvalidTriggerSettingsException(string triggerName, string message)
        : base($"Триггер {triggerName}: {message}")
    {
    }
}