using StudyOrganizer.Helper;
using StudyOrganizer.Object;

namespace StudyOrganizer;

public static class ProgramData
{
    public static void AssertSafeFileAccess()
    {
        if (!File.Exists(PathContainer.SettingsPath))
        {
            File.Create(PathContainer.SettingsPath);
        }

        if (!Directory.Exists(PathContainer.DataPath))
        {
            Directory.CreateDirectory(PathContainer.DataPath);
        }

        if (!File.Exists(PathContainer.CommandsPath))
        {
            File.Create(PathContainer.CommandsPath);
        }

        if (!File.Exists(PathContainer.DeadlinesPath))
        {
            File.Create(PathContainer.DeadlinesPath);
        }

        if (!File.Exists(PathContainer.LinksPath))
        {
            File.Create(PathContainer.LinksPath);
        }

        if (!File.Exists(PathContainer.UsersPath))
        {
            File.Create(PathContainer.UsersPath);
        }
    }

    private static void AssertNonEmptyContentForFile<T>(string path, T obj) {
        var file = new FileHelper(path);
        if (file.GetFileSize() != 0)
        {
            return;
        }

        var jsonSerializer = new JsonHelper(path);
        jsonSerializer.Serialize(obj);
    }
    
    public static void AssertNonEmptyContent()
    {
        AssertNonEmptyContentForFile(PathContainer.SettingsPath, new Settings());
        AssertNonEmptyContentForFile(PathContainer.CommandsPath, new List<Command>());
        AssertNonEmptyContentForFile(PathContainer.DeadlinesPath, new List<Deadline>());
        AssertNonEmptyContentForFile(PathContainer.LinksPath, new List<Link>());
        AssertNonEmptyContentForFile(PathContainer.UsersPath, new List<User>());
    }
}