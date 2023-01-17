using StudyOrganizer.Object;
using StudyOrganizer.Helper;

namespace StudyOrganizer;

public static class Initializer
{
    public static Settings LoadSettings()
    {
        var jsonSerializer = new JsonHelper(PathContainer.SettingsPath);
        
        var settings = jsonSerializer.Deserialize<Settings>();
        if (settings is null)
        {
            throw new ArgumentNullException($"Не удалось десериализовать настройки.");
        }
         
        
        return settings;
    }

    public static IList<Command> LoadCommands()
    {
        var jsonSerializer = new JsonHelper(PathContainer.CommandsPath);
        
        var commands = jsonSerializer.Deserialize<IList<Command>>();
        if (commands is null)
        {
            throw new ArgumentNullException($"Не удалось десериализовать команды.");
        }

        return commands;
    }

    public static IList<Deadline> LoadDeadlines()
    {
        var jsonSerializer = new JsonHelper(PathContainer.DeadlinesPath);

        var deadlines = jsonSerializer.Deserialize<IList<Deadline>>();
        if (deadlines is null)
        {
            throw new ArgumentNullException($"Не удалось десериализовать дедлайны.");
        }

        return deadlines;
    }

    public static IList<Link> LoadLinks()
    {
        var jsonSerializer = new JsonHelper(PathContainer.LinksPath);

        var links = jsonSerializer.Deserialize<List<Link>>();
        if (links is null)
        {
            throw new ArgumentNullException($"Не удалось десериализовать ссылки.");
        }

        return links;
    }

    public static IList<User> LoadUsers()
    {
        var jsonSerializer = new JsonHelper(PathContainer.UsersPath);

        var users = jsonSerializer.Deserialize<List<User>>();
        if (users is null)
        {
            throw new ArgumentNullException($"Не удалось десериализовать пользователей.");
        }

        return users;
    }
}