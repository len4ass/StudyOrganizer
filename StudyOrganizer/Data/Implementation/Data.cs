using StudyOrganizer.Data.Interface;

namespace StudyOrganizer.Data.Implementation;

public class Data<T> : IDataProvider<T>
{
    private readonly IList<T>? _data;

    public Data(IList<T> data)
    {
        _data = data;
    }
    
    public void Add(T data)
    {
        _data?.Add(data);
    }

    public void Remove(T data)
    {
        _data?.Remove(data);
    }

    public IReadOnlyList<T>? GetData()
    {
        return _data as IReadOnlyList<T>;
    }
}