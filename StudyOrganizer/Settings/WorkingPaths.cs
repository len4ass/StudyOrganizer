namespace StudyOrganizer.Settings;

public class WorkingPaths
{
    public string SettingsFile { get; }
    public string CommandsSettingsDirectory { get; }
    public string TriggersSettingsDirectory { get; }
    public string DataBaseFile { get; }

    public WorkingPaths(
        string settingsFile,
        string commandsSettingsDirectory,
        string triggersSettingsDirectory,
        string dataBaseFile)
    {
        SettingsFile = settingsFile;
        CommandsSettingsDirectory = commandsSettingsDirectory;
        TriggersSettingsDirectory = triggersSettingsDirectory;
        DataBaseFile = dataBaseFile;
    }
}