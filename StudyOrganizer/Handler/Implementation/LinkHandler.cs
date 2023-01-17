using StudyOrganizer.Data.Interface;
using StudyOrganizer.Handler.Interface;
using StudyOrganizer.Object;

namespace StudyOrganizer.Handler.Implementation;

public class LinkHandler : ILinkHandler
{
    private IDataProvider<Link> _linkData;

    public LinkHandler(IDataProvider<Link> linkData)
    {
        _linkData = linkData;
    }
}