namespace StudyOrganizer.Services.BotService.Responses;

public class UserResponse
{
    public string Response { get; }

    public UserResponse(string response)
    {
        Response = response;
    }
}