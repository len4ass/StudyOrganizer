using StudyOrganizer.Helper;
using StudyOrganizer.Helper.Serializers;
using StudyOrganizer.Settings;

namespace StudyOrganizer;

public static class ProgramData
{
    public static T LoadFrom<T>(string path)
    {
        var helper = new DataHelper<T>(new JsonHelper<T>(path));
        
        var data = helper.LoadData();
        ArgumentNullException.ThrowIfNull(data);
        return data;
    }

    
    public static void SaveTo<T>(string path, T data)
    {
        var helper = new DataHelper<T>(new JsonHelper<T>(path));
        helper.SaveData(data);
    }
}