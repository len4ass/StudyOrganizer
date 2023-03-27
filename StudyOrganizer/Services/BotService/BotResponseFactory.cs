using StudyOrganizer.Models.User;

namespace StudyOrganizer.Services.BotService;

public static class BotResponseFactory
{
    public static BotResponse NotEnoughArguments(
        string commandName, 
        string userHandle,
        long userId)
    {
        return new BotResponse(string.Format(Responses.NotEnoughArguments, commandName),
            string.Format(InternalResponses.BadRequest,
                userHandle,
                userId,
                commandName));
    }
    
    public static BotResponse ArgumentLimitExceeded(
        string commandName, 
        string userHandle,
        long userId)
    {
        return new BotResponse(string.Format(Responses.ArgumentLimitExceeded, commandName),
            string.Format(
                InternalResponses.BadRequest,
                userHandle,
                userId,
                commandName));
    }

    public static BotResponse FailedParsing(
        string commandName, 
        string userHandle,
        long userId)
    {
        return new BotResponse(string.Format(Responses.FailedToParseArguments, commandName),
            string.Format(
                InternalResponses.Failed,
                userHandle,
                userId,
                commandName));
    }

    public static BotResponse EntryAlreadyExists(
        string commandName, 
        string entryModelName,
        string entryName,
        string userHandle,
        long userId)
    {
        return new BotResponse(
            string.Format(
                Responses.EntryAlreadyExists,
                commandName,
                entryModelName,
                entryName), 
            string.Format(
                InternalResponses.Failed,
                userHandle,
                userId,
                commandName));
    }

    public static BotResponse EntryDoesNotExist(
        string commandName, 
        string entryModelName, 
        string entryName, 
        string userHandle, 
        long userId)
    {
        return new BotResponse(
            string.Format(
                Responses.EntryDoesNotExist,
                commandName,
                entryModelName,
                entryName), 
            string.Format(
                InternalResponses.Failed,
                userHandle,
                userId,
                commandName));
    }

    public static BotResponse Success(
        string commandName, 
        string userResponse, 
        string userHandle,
        long userId)
    {
        return new BotResponse(userResponse,
            string.Format(
                InternalResponses.Success,
                userHandle,
                userId,
                commandName));
    }

    public static BotResponse FailedAnalyzingText(
        string commandName,
        string userHandle,
        long userId)
    {
        return new BotResponse(
            string.Format(
                Responses.FailedAnalizingText, 
                commandName),
            string.Format(
                InternalResponses.Failed,
                userHandle,
                userId,
                commandName));
    }
}