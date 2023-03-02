namespace StudyOrganizer.Settings;

public class WorkingPaths
{
    public string TokenFile { get; }
    public string SettingsFile { get; }
    public string CommandsDirectory { get; }
    public string CommandsSettingsDirectory { get; }
    public string TriggersDirectory { get; }
    public string TriggersSettingsDirectory { get; }

    public WorkingPaths(
        string tokenFile, 
        string settingsFile, 
        string commandsDirectory, 
        string commandsSettingsDirectory, 
        string triggersDirectory, 
        string triggersSettingsDirectory)
    {
        TokenFile = tokenFile;
        SettingsFile = settingsFile;
        CommandsDirectory = commandsDirectory;
        CommandsSettingsDirectory = commandsSettingsDirectory;
        TriggersDirectory = triggersDirectory;
        TriggersSettingsDirectory = triggersSettingsDirectory;
    }
}