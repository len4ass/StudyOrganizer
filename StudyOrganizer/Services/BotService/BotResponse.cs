namespace StudyOrganizer.Services.BotService;

public class BotResponse
{
    public string UserResponse { get; }
    public string InternalResponse { get; }

    public BotResponse(string userResponse, string internalResponse)
    {
        UserResponse = userResponse;
        InternalResponse = internalResponse;
    }
}