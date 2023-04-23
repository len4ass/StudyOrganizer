namespace StudyOrganizer.Services.BotService;

public static class UserResponses
{
    public const string CommandDoesNotExist = "Команды <b>/{0}</b> не существует, попробуйте заново.";

    public const string AccessDenied = "Не удалось использовать команду <b>{0}</b>: у вас недостаточно прав.";

    public const string NotEnoughArguments = "Не удалось использовать команду <b>{0}</b>: недостаточно аргументов.";

    public const string ArgumentLimitExceeded =
        "Не удалось использовать команду <b>{0}</b>: превышено количество аргументов.";

    public const string FailedToParseArguments =
        "Не удалось использовать команду <b>{0}</b>: не удалось пропарсить аргументы.";

    public const string FailedToParseArgumentsSpecified =
        "Не удалось использовать команду <b>{0}</b>: не удалось пропарсить аргументы. \n{1}";

    public const string CommandDoesNotExistInDatabase =
        "Не удалось использовать команду <b>{0}</b>: команда с именем {1} не существует.";

    public const string TriggerDoesNotExistInDatabase =
        "Не удалось использовать команду <b>{0}</b>: триггер с именем {1} не существует.";

    public const string UserAlreadyExists =
        "Не удалось использовать команду <b>{0}</b>: пользователь {1} уже существует.";

    public const string UserDoesNotExist =
        "Не удалось использовать команду <b>{0}</b>: пользователь {1} не существует.";

    public const string LinkAlreadyExists =
        "Не удалось использовать команду <b>{0}</b>: ссылка {1} уже существует.";

    public const string LinkDoesNotExist =
        "Не удалось использовать команду <b>{0}</b>: ссылка {1} не существует.";

    public const string DeadlineAlreadyExists =
        "Не удалось использовать команду <b>{0}</b>: дедлайн {1} уже существует.";

    public const string DeadlineDoesNotExist =
        "Не удалось использовать команду <b>{0}</b>: дедлайн {1} не существует.";

    public const string FailedAnalizingText =
        "Не удалось использовать команду <b>{0}</b>: не удалось проанализировать ваше сообщение.";

    public const string ApiRequestFailed =
        "Не удалось использовать команду <b>{0}</b>: произошла ошибка API телеграма.";

    public const string CanNotChangeOwnData =
        "Не удалось использовать команду <b>{0}</b>: нельзя менять информацию о самом себе.";
}