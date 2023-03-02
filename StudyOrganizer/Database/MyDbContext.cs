using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Models.Command;
using StudyOrganizer.Models.Deadline;
using StudyOrganizer.Models.Link;
using StudyOrganizer.Models.Trigger;
using StudyOrganizer.Models.User;
using StudyOrganizer.Services.BotService.Command;

namespace StudyOrganizer.Database;

public sealed class MyDbContext : DbContext
{
    public DbSet<CommandInfo> Commands => Set<CommandInfo>();
    public DbSet<UserInfo> Users => Set<UserInfo>();
    public DbSet<DeadlineInfo> Deadlines => Set<DeadlineInfo>();
    public DbSet<LinkInfo> Links => Set<LinkInfo>();
    public DbSet<TriggerInfo> Triggers => Set<TriggerInfo>();

    public MyDbContext()
    {
        Database.EnsureCreated();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder)
    {
        dbContextOptionsBuilder.UseSqlite("Data Source=bot_data.db");
    }
}