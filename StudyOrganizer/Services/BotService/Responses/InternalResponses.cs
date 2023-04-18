namespace StudyOrganizer.Services.BotService.Responses;

public static class InternalResponses
{
    public const string Success = "Пользователь {0} ({1}) воспользовался командой {2}.";
    public const string BadRequest = "Пользователь {0} ({1}) неверно применил команду {2}.";
    public const string AccessDenied = "У пользователя {0} ({1}) недостаточно прав для использования команды {2}.";
    public const string Failed = "При обработке сообщения от пользователя {0} ({1}) произошла ошибка на команде {2}.";
    public const string CommandDoesNotExist = "Пользователь {0} ({1}) применил несуществующую команду {2}.";

    public const string InternalServerError =
        "При обработке сообщения от пользователя {0} ({1}) произошла ошибка на команде {2}: {3}";
}