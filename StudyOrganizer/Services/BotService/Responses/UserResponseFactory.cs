namespace StudyOrganizer.Services.BotService.Responses;

public static class UserResponseFactory
{
    public static UserResponse FailedParsingSpecified(string commandName, string parsingError)
    {
        return new UserResponse
        {
            Response = string.Format(
                UserResponses.FailedToParseArgumentsSpecified,
                commandName,
                parsingError)
        };
    }

    public static UserResponse CommandDoesNotExist(string commandName)
    {
        return new UserResponse
        {
            Response = string.Format(UserResponses.CommandDoesNotExist, commandName)
        };
    }

    public static UserResponse AccessDenied(string commandName)
    {
        return new UserResponse
        {
            Response = string.Format(UserResponses.AccessDenied, commandName)
        };
    }

    public static UserResponse NotEnoughArguments(string commandName)
    {
        return new UserResponse
        {
            Response = string.Format(UserResponses.NotEnoughArguments, commandName)
        };
    }

    public static UserResponse ArgumentLimitExceeded(string commandName)
    {
        return new UserResponse
        {
            Response = string.Format(UserResponses.ArgumentLimitExceeded, commandName)
        };
    }

    public static UserResponse FailedParsing(string commandName)
    {
        return new UserResponse
        {
            Response = string.Format(UserResponses.FailedToParseArguments, commandName)
        };
    }

    public static UserResponse FailedSendingApiRequest(string commandName)
    {
        return new UserResponse
        {
            Response = string.Format(UserResponses.ApiRequestFailed, commandName)
        };
    }

    public static UserResponse EntryAlreadyExists(
        string commandName,
        string entryModelName,
        string entryName)
    {
        return new UserResponse
        {
            Response = string.Format(
                UserResponses.EntryAlreadyExists,
                commandName,
                entryModelName,
                entryName)
        };
    }

    public static UserResponse InvalidUri(string commandName)
    {
        return new UserResponse
        {
            Response = string.Format(UserResponses.InvalidUri, commandName)
        };
    }

    public static UserResponse EntryDoesNotExist(
        string commandName,
        string entryModelName,
        string entryName)
    {
        return new UserResponse
        {
            Response = string.Format(
                UserResponses.EntryDoesNotExist,
                commandName,
                entryModelName,
                entryName)
        };
    }

    public static UserResponse Success(string userResponse)
    {
        return new UserResponse
        {
            Response = userResponse
        };
    }

    public static UserResponse FailedAnalyzingText(string commandName)
    {
        return new UserResponse
        {
            Response = string.Format(UserResponses.FailedAnalizingText, commandName)
        };
    }

    public static UserResponse FailedChangingOwnData(string commandName)
    {
        return new UserResponse
        {
            Response = string.Format(UserResponses.CanNotChangeOwnData, commandName)
        };
    }
}