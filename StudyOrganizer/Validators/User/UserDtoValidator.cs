using FluentValidation;
using StudyOrganizer.Models.User;

namespace StudyOrganizer.Validators.User;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    public UserDtoValidator()
    {
        RuleFor(user => user.WonCOTD)
            .GreaterThanOrEqualTo(0)
            .WithMessage(
                "Некорректное значение {PropertyName}: {PropertyValue}. " +
                "Количество раз, которое пользователь стал красавчиком дня, должно быть больше или равно 0.");
        RuleFor(user => user.MsgAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(
                "Некорректное значение {PropertyName}: {PropertyValue}. " +
                "Количество сообщений пользователя должно быть больше или равно 0.");
    }
}