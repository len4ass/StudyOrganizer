namespace StudyOrganizer.Helper;

public class FileHelper
{
    private readonly string _path;

    public FileHelper(string path)
    {
        _path = path;
    }

    public long GetFileSize()
    {
        var file = new FileInfo(_path);
        return file.Length;
    }
}