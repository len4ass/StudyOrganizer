using StudyOrganizer.Helper;
using StudyOrganizer.Helper.Serializers;
using StudyOrganizer.Loaders;
using StudyOrganizer.Models.Deadline;
using StudyOrganizer.Models.Link;
using StudyOrganizer.Models.User;
using StudyOrganizer.Settings;

namespace StudyOrganizer;

public static class ProgramData
{
    public static T LoadFrom<T>(string path)
    {
        var loader = new DataHelper<T>(new JsonHelper<T>(path));
        
        var data = loader.LoadData();
        if (data is null)
        {
            throw new ArgumentNullException($"Не удалось десериализовать данные типа {typeof(T)}.");
        }

        return data;
    }
    
    
    public static void AssertSafeFileAccess()
    {
        if (!File.Exists(PathContainer.SettingsPath))
        {
            var handle = File.Create(PathContainer.SettingsPath);
            handle.Close();
        }

        if (!Directory.Exists(PathContainer.DataDirectory))
        {
            Directory.CreateDirectory(PathContainer.DataDirectory);
        }

        if (!File.Exists(PathContainer.DeadlinesPath))
        {
            var handle = File.Create(PathContainer.DeadlinesPath);
            handle.Close();
        }

        if (!File.Exists(PathContainer.LinksPath))
        {
            var handle = File.Create(PathContainer.LinksPath);
            handle.Close();
        }

        if (!File.Exists(PathContainer.UsersPath))
        {
            var handle = File.Create(PathContainer.UsersPath);
            handle.Close();
        }
    }

    private static bool AssertNonEmptyContentForFile<T>(string path, T obj) {
        var file = new FileHelper(path);
        if (file.GetFileSize() != 0)
        {
            return true;
        }

        var jsonSerializer = new JsonHelper<T>(path);
        jsonSerializer.Serialize(obj);
        return false;
    }

    public static void AssertNonEmptyContent()
    {
        AssertNonEmptyContentForFile(PathContainer.SettingsPath, new GeneralSettings());
        AssertNonEmptyContentForFile(PathContainer.DeadlinesPath, new List<DeadlineInfo>());
        AssertNonEmptyContentForFile(PathContainer.LinksPath, new List<LinkInfo>());
        AssertNonEmptyContentForFile(PathContainer.UsersPath, new List<UserInfo>());
    }
}