using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Models.User;

namespace StudyOrganizer.Settings;

[Owned]
public sealed class CommandSettings
{
    public AccessLevel AccessLevel { get; init; }
}