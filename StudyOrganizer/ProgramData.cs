using StudyOrganizer.Helper;
using StudyOrganizer.Helper.Serializers;
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
    }
    
    public static void ValidateSettings(GeneralSettings generalSettings)
    {
        ArgumentNullException.ThrowIfNull(generalSettings.Token);
        

        if (generalSettings.OwnerId == 0)
        {
            throw new InvalidDataException(
                "Укажите ID владельца бота (OwnerId) в settings.json в папке с программой.");
        }

        if (generalSettings.MainChatId == 0)
        {
            throw new InvalidDataException(
                "Укажите ID основного чата (MainChatId) в settings.json в папке с программой.");
        }

        if (generalSettings.ImportantChatId == 0)
        {
            throw new InvalidDataException(
                $"Укажите ID важного чата (ImportantChatId) в settings.json в папке с программой.");
        }

        /*if (generalSettings.ChatTimeZoneUtc is null)
        {
            throw new InvalidDataException(
                "Укажите тайм-зону основного чата (ChatTimeZoneUtc) в settings.json в папке с программой.");
        }*/
    }
}