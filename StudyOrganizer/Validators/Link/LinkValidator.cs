using FluentValidation;
using StudyOrganizer.Models.Link;

namespace StudyOrganizer.Validators.Link;

public class LinkValidator : AbstractValidator<LinkInfo>
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

    public LinkValidator()
    {
        RuleFor(link => link.Name)
            .Matches("^[A-Za-z0-9_\u0400-\u04FF]*$")
            .WithMessage(
                "Имя ссылки содержит недопустимые символы. " +
                "Допустимые символы: символы русского/английского алфавита, цифры и нижнее подчеркивание.");

        RuleFor(link => link.Uri)
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("Ссылка имеет некорректный формат.");

        RuleFor(link => link.Description)
            .Must(description => InvalidCharactersInDescription.All(c => !description.Contains(c)))
            .WithMessage(
                $"Описание содержит недопустимые символы. Список недопустимых символов: {InvalidCharactersRepresentation}");
    }
}