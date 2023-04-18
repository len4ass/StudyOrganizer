using StudyOrganizer.Helper;
using StudyOrganizer.Helper.Serializers;

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

    public static async Task<T> LoadFromAsync<T>(string path)
    {
        var helper = new DataHelper<T>(new JsonHelper<T>(path));

        var data = await helper.LoadDataAsync();
        ArgumentNullException.ThrowIfNull(data);
        return data;
    }

    public static async Task SaveToAsync<T>(string path, T data)
    {
        var helper = new DataHelper<T>(new JsonHelper<T>(path));
        await helper.SaveDataAsync(data);
    }
}