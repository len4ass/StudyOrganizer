using System.ComponentModel.DataAnnotations;
using Mapster;

namespace StudyOrganizer.Models.User;

[AdaptTwoWays(typeof(UserDto))]
public sealed class UserInfo
{
    [Key] public long Id { get; init; }
    public string Name { get; set; }
    public string? Handle { get; set; }
    public long MsgAmount { get; set; }
    public AccessLevel Level { get; set; }
    public bool CoolestOfTheDay { get; set; }
    public int WonCOTD { get; set; }
    public DateOnly? Birthday { get; set; }

    public UserInfo()
    {
    }

    public string GetBirthdayString()
    {
        if (!Birthday.HasValue)
        {
            return "не указано";
        }

        return $"{Birthday:dd.MM.yyyy}";
    }

    public DateTimeOffset GetBirthdayUtc(TimeZoneInfo timeZoneInfo)
    {
        if (!Birthday.HasValue)
        {
            return new DateTimeOffset();
        }

        var birthday = Birthday.Value.ToDateTime(
            new TimeOnly(
                0,
                0,
                0));
        return TimeZoneInfo.ConvertTimeToUtc(birthday, timeZoneInfo);
    }

    public int GetAge(TimeZoneInfo timeZoneInfo)
    {
        if (!Birthday.HasValue)
        {
            return -1;
        }

        var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
        var birthday = Birthday.Value.ToDateTime(
            new TimeOnly(
                0,
                0,
                0));

        var todayIntRepr = (today.Year * 100 + today.Month) * 100 + today.Day;
        var birthdayIntRepr = (birthday.Year * 100 + birthday.Month) * 100 + birthday.Day;
        return (todayIntRepr - birthdayIntRepr) / 10000;
    }

    public string GetCoolestOfTheDayString()
    {
        return CoolestOfTheDay ? "да" : "нет";
    }

    public string GetCorrectTagFormatting()
    {
        if (Handle is null)
        {
            return $"<a href=\"tg://user?id={Id}\"><b>{Name}</b></a>";
        }

        return $"@{Handle}";
    }

    public override string ToString()
    {
        return $"<b>Имя</b>: {Name}\n" +
               $"<b>Хэндл</b>: {Handle}\n" +
               $"<b>Идентификатор</b>: {Id}\n" +
               $"<b>Уровень доступа</b>: {Level}\n" +
               $"<b>Количество сообщений</b>: {MsgAmount}\n" +
               $"<b>Был красавчиком дня</b>: {WonCOTD} раз(а)\n" +
               $"<b>Красавчик сегодня</b>: {GetCoolestOfTheDayString()}\n" +
               $"<b>День рождения</b>: {GetBirthdayString()}\n";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not UserInfo user)
        {
            return false;
        }

        return Equals(user);
    }

    private bool Equals(UserInfo other)
    {
        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        var hashCode = Id.GetHashCode();
        return hashCode;
    }
}