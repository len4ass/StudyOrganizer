using Mapster;

namespace StudyOrganizer.Models.User;

[AdaptTwoWays(typeof(UserInfo))]
public class UserDto
{
    public long MsgAmount { get; set; }
    public AccessLevel Level { get; set; }
    public bool CoolestOfTheDay { get; set; }
    public int WonCOTD { get; set; }
    public DateOnly? Birthday { get; set; }
}