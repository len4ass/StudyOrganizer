using FluentValidation;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings;
using StudyOrganizer.Settings.SimpleTrigger;

namespace StudyOrganizer.Validators.Trigger;

public class TriggerSettingsValidator : AbstractValidator<TriggerSettings>
{
    private static readonly string[] ValidDays =
    {
        "MON",
        "TUE",
        "WED",
        "THU",
        "FRI",
        "SAT",
        "SUN"
    };

    private static readonly string ValidDaysRepresentation = $"[{string.Join(", ", ValidDays)}]";

    public TriggerSettingsValidator()
    {
        RuleFor(trigger => trigger.HourUtc)
            .InclusiveBetween(0, 23)
            .WithMessage(
                "Некорректное значение {PropertyName}: {PropertyValue}. Значение {PropertyName} должно быть в рамках [0, 23].");

        RuleFor(trigger => trigger.MinuteUtc)
            .InclusiveBetween(0, 59)
            .WithMessage(
                "Некорректное значение {PropertyName}: {PropertyValue}. Значение {PropertyName} должно быть в рамках [0, 59].");

        RuleFor(trigger => trigger.SecondUtc)
            .InclusiveBetween(0, 59)
            .WithMessage(
                "Некорректное значение {PropertyName}: {PropertyValue}. Значение {PropertyName} должно быть в рамках [0, 59].");
    }
}