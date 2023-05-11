# StudyOrganizer

Телеграм бот учебный органайзер

## Что он может
1) Добавлять, обновлять, удалять и показывать дедлайны, рассылать уведомления о них
2) Добавлять, обновлять, удалять и показывать ссылки
3) Добавлять, обновлять, удалять и показывать пользователей
4) Рассылать уведомления 
5) Изменять настройки уведомлений и команд в рантайме
6) Обрабатывать естественный текст в команды
7) Обрабатывать голосовые сообщения в команды 

## Как запустить

### Docker
1. Клонировать репозиторий `git clone https://github.com/len4ass/StudyOrganizer.git`
2. Перейти в директорию репозитория 
2. Собрать образ `docker build -t study_organizer -f Dockerfile .`
3. Создать директорию `study_organizer_bot` и следующие директории внутри нее для хранения данных бота локально (желательно в другой директории, нежели та, в которую было произведено клонирование репозитория):
   * `command_settings`
   * `trigger_settings`
   * `db`
4. Создать файл `settings.json` в директории `study_organizer_bot` с следующим содержимым:
    ```json
    {
      "OwnerId": ...,
      "MainChatId": ...,
      "ImportantChatId": ...,
      "ChatTimeZoneUtc": "..."
    }
    ```
   * `OwnerId` - идентификатор владельца бота в телеграм
   * `MainChatId` - идентификатор основного чата, в котором происходит основное общение пользователей
   * `ImportantChatId` - идентификатор важного чата, в который будет производится отправка уведомлений (этот чат может быть тем же, что и MainChatId)
   * `ChatTimeZoneUtc` - таймзона чата. Если вы живете по московскому времени, нужно указать `Russian Standard Time`. [Другие таймзоны (см. колонку TimeZone).](https://learn.microsoft.com/en-us/windows-hardware/manufacture/desktop/default-time-zones?view=windows-11)
5. Запустить контейнер с маунтом к файловой системе:
   `docker run -v /path/to/study_organizer_bot:/app/out --restart unless-stopped --name SORG -itd --network=host -e BOT_TOKEN=... -e YANDEX_CLOUD_TOKEN=... -e OPEN_API_TOKEN=... study_organizer out/settings.json out/command_settings/ out/trigger_settings/ out/db/bot_data.db`
   * `/path/to/study_organizer_bot` - абсолютный путь к директории `study_organizer_bot`
   * `BOT_TOKEN` - токен телеграм бота, выданный с помощью BotFather
   * `YANDEX_CLOUD_TOKEN` - токен для взаимодействия с Yandex Cloud Speech Kit API (необязательно)
   * `OPEN_API_TOKEN` - токен для взаимодействия с Open AI API (необязательно)
6. Убедиться, что бот добавлен в чаты `MainChatId` и `ImportantChatId` и пользоваться им.