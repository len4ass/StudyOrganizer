using StudyOrganizer.Models.Link;

namespace StudyOrganizer.Repositories.Link;

public class LinkInfoRepository : ILinkInfoRepository
{
    private IList<LinkInfo> _linkData;

    public LinkInfoRepository(IList<LinkInfo> linkData)
    {
        _linkData = linkData;
    }

    public bool Add(LinkInfo element)
    {
        throw new NotImplementedException();
    }

    public bool Remove(LinkInfo element)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<LinkInfo> GetData()
    {
        return _linkData.ToList().AsReadOnly();
    }

    public LinkInfo? Find(string element)
    {
        throw new NotImplementedException();
    }
}