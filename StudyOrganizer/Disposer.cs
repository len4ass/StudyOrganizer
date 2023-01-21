using StudyOrganizer.Helper;
using StudyOrganizer.Helper.Serializers;

namespace StudyOrganizer;

public static class Disposer<T>
{
    public static void SaveTo(T obj, string path)
    {
        var loader = new DataHelper<T>(new JsonHelper<T>(path));
        
        loader.SaveData(obj);
    }
}