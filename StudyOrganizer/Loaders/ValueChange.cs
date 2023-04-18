namespace StudyOrganizer.Loaders;

public class ValueChange
{
    public string Name { get; }
    public object? PreviousValue { get; }
    public object? CurrentValue { get; }

    public ValueChange(
        string name,
        object? previousValue,
        object? currentValue)
    {
        Name = name;
        PreviousValue = previousValue;
        CurrentValue = currentValue;
    }

    public override string ToString()
    {
        return $"{Name} изменено с {PreviousValue} на {CurrentValue}";
    }
}