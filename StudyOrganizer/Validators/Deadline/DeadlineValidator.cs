using FluentValidation;
using StudyOrganizer.Models.Deadline;

namespace StudyOrganizer.Validators.Deadline;

public class DeadlineValidator : AbstractValidator<DeadlineInfo>
{
    private static readonly char[] InvalidCharactersInDescription =
    {
        '&',
        '<',
        '>',
        '\'',
        '"'
    };

    private static readonly string InvalidCharactersRepresentation =
        $"[{string.Join(", ", InvalidCharactersInDescription)}]";

    public DeadlineValidator()
    {
        RuleFor(deadline => deadline.Name)
            .Matches("^[A-Za-z0-9_\u0400-\u04FF]*$")
            .WithMessage(
                "Имя дедлайны содержит недопустимые символы. " +
                "Допустимые символы: символы русского/английского алфавита, цифры и нижнее подчеркивание.");

        RuleFor(deadline => deadline.DateUtc)
            .Must(dateUtc => dateUtc >= DateTimeOffset.UtcNow)
            .WithMessage("Дедлайн уже истек.");

        RuleFor(deadline => deadline.Description)
            .Must(description => InvalidCharactersInDescription.All(c => !description.Contains(c)))
            .WithMessage(
                $"Описание содержит недопустимые символы. Список недопустимых символов: {InvalidCharactersRepresentation}");
    }
}