using Mapster;
using StudyOrganizer.Loaders;

namespace StudyOrganizer.Models.User;

[AdaptTwoWays(typeof(UserInfo))]
public class UserDto
{
    public long MsgAmount { get; set; }
    public AccessLevel Level { get; set; }
    public bool CoolestOfTheDay { get; set; }
    public int WonCOTD { get; set; }
    public DateOnly? Birthday { get; set; }

    public static UserDto GetUserDtoFromKeyValuePairs(IDictionary<string, string> keyValuePairs)
    {
        var userDto = new UserDto();
        ReflectionHelper.ParseAndMapKeyValuePairsOnObject(userDto, keyValuePairs);
        return userDto;
    }
}