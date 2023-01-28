namespace StudyOrganizer.Services.BotService;

public static class Responses
{
    public const string CommandDoesNotExist = "Команды /{0} не существует, попробуйте заново.";
    public const string AccessDenied = "Не удалось использовать команду {0}: у вас недостаточно прав.";
    public const string NotEnoughArguments = "Не удалось использовать команду {0}: недостаточно аргументов.";
    public const string ArgumentLimitExceeded = "Не удалось использовать команду {0}: превышено количество аргументов.";
}