using StudyOrganizer.Bot;
using StudyOrganizer.Data.Implementation;
using StudyOrganizer.Handler.Implementation;
using StudyOrganizer.Object;

namespace StudyOrganizer;

public class ProgramRunner
{
    public void Run()
    {
        ProgramData.AssertSafeFileAccess();
        ProgramData.AssertNonEmptyContent();

        var settings = Initializer.LoadSettings();
        var commandHandler = new CommandHandler(new Data<Command>(Initializer.LoadCommands()));
        var deadlineHandler = new DeadlineHandler(new Data<Deadline>(Initializer.LoadDeadlines()));
        var linkHandler = new LinkHandler(new Data<Link>(Initializer.LoadLinks()));
        var userHandler = new UserHandler(new Data<User>(Initializer.LoadUsers()));

        var mainHandler = new MainHandler();
        mainHandler.RegisterHandler("command", commandHandler);
        mainHandler.RegisterHandler("deadline", deadlineHandler);
        mainHandler.RegisterHandler("link", linkHandler);
        mainHandler.RegisterHandler("user", userHandler);

        var botService = new BotService(mainHandler, settings);
        botService.StartService();
    }
}

