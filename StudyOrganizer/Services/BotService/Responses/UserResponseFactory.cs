namespace StudyOrganizer.Services.BotService.Responses;

public static class UserResponseFactory
{
    public static UserResponse CommandDoesNotExist(string commandName)
    {
        return new UserResponse(string.Format(UserResponses.CommandDoesNotExist, commandName));
    }

    public static UserResponse AccessDenied(string commandName)
    {
        return new UserResponse(string.Format(UserResponses.AccessDenied, commandName));
    }

    public static UserResponse NotEnoughArguments(string commandName)
    {
        return new UserResponse(string.Format(UserResponses.NotEnoughArguments, commandName));
    }

    public static UserResponse ArgumentLimitExceeded(string commandName)
    {
        return new UserResponse(string.Format(UserResponses.ArgumentLimitExceeded, commandName));
    }

    public static UserResponse FailedParsing(string commandName)
    {
        return new UserResponse(string.Format(UserResponses.FailedToParseArguments, commandName));
    }

    public static UserResponse FailedParsingSpecified(string commandName, string parsingError)
    {
        return new UserResponse(
            string.Format(
                UserResponses.FailedToParseArgumentsSpecified,
                commandName,
                parsingError));
    }

    public static UserResponse FailedSendingApiRequest(string commandName)
    {
        return new UserResponse(string.Format(UserResponses.ApiRequestFailed, commandName));
    }

    public static UserResponse UserAlreadyExists(string commandName, string user)
    {
        return new UserResponse(
            string.Format(
                UserResponses.UserAlreadyExists,
                commandName,
                user));
    }

    public static UserResponse UserDoesNotExist(string commandName, string user)
    {
        return new UserResponse(
            string.Format(
                UserResponses.UserDoesNotExist,
                commandName,
                user));
    }

    public static UserResponse CommandDoesNotExistInDatabase(string commandName, string command)
    {
        return new UserResponse(
            string.Format(
                UserResponses.CommandDoesNotExistInDatabase,
                commandName,
                command));
    }

    public static UserResponse TriggerDoesNotExistInDatabase(string commandName, string trigger)
    {
        return new UserResponse(
            string.Format(
                UserResponses.TriggerDoesNotExistInDatabase,
                commandName,
                trigger));
    }

    public static UserResponse LinkAlreadyExists(string commandName, string link)
    {
        return new UserResponse(
            string.Format(
                UserResponses.LinkAlreadyExists,
                commandName,
                link));
    }

    public static UserResponse LinkDoesNotExist(string commandName, string link)
    {
        return new UserResponse(
            string.Format(
                UserResponses.LinkDoesNotExist,
                commandName,
                link));
    }

    public static UserResponse DeadlineAlreadyExists(string commandName, string deadline)
    {
        return new UserResponse(
            string.Format(
                UserResponses.DeadlineAlreadyExists,
                commandName,
                deadline));
    }

    public static UserResponse DeadlineDoesNotExist(string commandName, string deadline)
    {
        return new UserResponse(
            string.Format(
                UserResponses.DeadlineDoesNotExist,
                commandName,
                deadline));
    }

    public static UserResponse Success(string userResponse)
    {
        return new UserResponse(userResponse);
    }

    public static UserResponse FailedAnalyzingText(string commandName)
    {
        return new UserResponse(string.Format(UserResponses.FailedAnalizingText, commandName));
    }

    public static UserResponse FailedChangingOwnData(string commandName)
    {
        return new UserResponse(string.Format(UserResponses.CanNotChangeOwnData, commandName));
    }
}