namespace StudyOrganizer.Services.BotService;

public static class Responses
{
    public const string CommandDoesNotExist = "Команды <b>/{0}</b> не существует, попробуйте заново.";
    public const string AccessDenied = "Не удалось использовать команду <b>{0}</b>: у вас недостаточно прав.";
    public const string NotEnoughArguments = "Не удалось использовать команду <b>{0}</b>: недостаточно аргументов.";
    public const string ArgumentLimitExceeded = "Не удалось использовать команду <b>{0}</b>: превышено количество аргументов.";
    public const string FailedToParseArguments =
        "Не удалось использовать команду <b>{0}</b>: не удалось пропарсить аргументы.";

    public const string EntryAlreadyExists = "Не удалось использовать команду <b>{0}</b>: {1} {2} уже существует.";
    public const string EntryDoesNotExist = "Не удалось использовать команду <b>{0}</b>: {1} {2} не существует.";

    public const string FailedAnalizingText =
        "Не удалось использовать команду <b>{0}</b>: не удалось проанализировать ваше сообщение";

    public const string InvalidUri = "Не удалось использовать команду <b>{0}</b>: указана некорректная ссылка.";
}