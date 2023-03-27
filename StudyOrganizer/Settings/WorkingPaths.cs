namespace StudyOrganizer.Settings;

public class WorkingPaths
{
    public string BotTokenFile { get; }
    public string OpenApiTokenFile { get; }
    public string YandexCloudApiTokenFile { get; }
    public string SettingsFile { get; }
    public string CommandsDirectory { get; }
    public string CommandsSettingsDirectory { get; }
    public string TriggersDirectory { get; }
    public string TriggersSettingsDirectory { get; }
    public string DataBaseFile { get; }

    public WorkingPaths(
        string botTokenFile,
        string openApiTokenFile,
        string yandexCloudApiTokenFile,
        string settingsFile, 
        string commandsDirectory, 
        string commandsSettingsDirectory, 
        string triggersDirectory, 
        string triggersSettingsDirectory,
        string dataBaseFile)
    {
        BotTokenFile = botTokenFile;
        OpenApiTokenFile = openApiTokenFile;
        YandexCloudApiTokenFile = yandexCloudApiTokenFile;
        SettingsFile = settingsFile;
        CommandsDirectory = commandsDirectory;
        CommandsSettingsDirectory = commandsSettingsDirectory;
        TriggersDirectory = triggersDirectory;
        TriggersSettingsDirectory = triggersSettingsDirectory;
        DataBaseFile = dataBaseFile;
    }
}