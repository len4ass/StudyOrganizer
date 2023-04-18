using System.ComponentModel;
using System.Reflection;
using Mapster;
using StudyOrganizer.Models.User;

namespace StudyOrganizer.Loaders;

public static class ReflectionHelper
{
    public static Type? GetTypeFromAssembly(string path)
    {
        var properPath = Path.GetFullPath(path);
        var fileName = Path.GetFileNameWithoutExtension(properPath);
        var assembly = Assembly.LoadFrom(properPath);

        return assembly.GetType($"{fileName}.{fileName}");
    }

    public static object? CreateTypeInstance(Type type, params object[] constructorParameters)
    {
        return Activator.CreateInstance(type, constructorParameters);
    }

    public static FieldInfo? FindField(object? instance, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(instance);
        var fields = GetFields(instance.GetType());
        foreach (var field in fields)
        {
            if (field.Name == fieldName)
            {
                return field;
            }
        }

        return null;
    }

    public static PropertyInfo? FindProperty(object? instance, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(instance);
        var properties = GetProperties(instance.GetType());
        foreach (var property in properties)
        {
            if (property.Name == propertyName)
            {
                return property;
            }
        }

        return null;
    }

    public static FieldInfo[] GetFields(Type type)
    {
        return type.GetFields(
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance);
    }

    public static PropertyInfo[] GetProperties(Type type)
    {
        return type.GetProperties(
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance);
    }

    public static IList<string> GetPropertyNames(Type type)
    {
        var properties = GetProperties(type);

        return properties.Select(property => property.Name)
            .ToList();
    }

    public static void ParseAndMapKeyValuePairsOnObject<T>(T obj, IDictionary<string, string> propertyNameValuePairs)
    {
        var properties = GetProperties(typeof(T));
        foreach (var property in properties)
        {
            if (!propertyNameValuePairs.ContainsKey(property.Name))
            {
                continue;
            }

            var converter = TypeDescriptor.GetConverter(property.PropertyType);
            var result = converter.ConvertFrom(propertyNameValuePairs[property.Name]);
            if (result is null)
            {
                continue;
            }

            property.SetValue(obj, result);
        }
    }

    public static IList<string> GetPropertyNamesWithValues<T>(T obj)
    {
        var properties = GetProperties(typeof(T));

        return properties.Select(property => $"{property.Name}: {property.GetValue(obj)}")
            .ToList();
    }

    public static IList<string> GetPropertyNamesWithTypes(Type type)
    {
        var properties = GetProperties(type);

        return properties
            .Select(property => $"{property.Name} ({property.PropertyType.Name})")
            .ToList();
    }

    public static object? GetStaticFieldValue(Type type, string fieldName)
    {
        var fields = GetFields(type);
        object? value = null;
        var foundField = false;
        foreach (var field in fields)
        {
            if (field.Name == fieldName)
            {
                value = field.GetValue(null);
                foundField = true;
                break;
            }
        }

        if (!foundField)
        {
            throw new MissingFieldException($"Поле с именем {fieldName} у типа {type.Name} не найдено");
        }

        return value;
    }

    public static object? GetPropertyValue(object? instance, string propertyName)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance), "Переданный объект null.");
        }

        var properties = GetProperties(instance.GetType());
        object? value = null;
        var foundProperty = false;
        foreach (var property in properties)
        {
            if (property.Name == propertyName)
            {
                value = property.GetValue(instance);
                foundProperty = true;
                break;
            }
        }

        if (!foundProperty)
        {
            throw new MissingFieldException(
                $"Свойство с именем {propertyName} у типа {instance.GetType().Name} не найдено");
        }

        return value;
    }

    public static void SetPropertyValue(
        object? instance,
        object? value,
        string propertyName)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance), "Переданный объект null.");
        }

        var properties = GetProperties(instance.GetType());
        foreach (var property in properties)
        {
            if (property.Name == propertyName)
            {
                property.SetValue(instance, value);
                break;
            }
        }
    }

    public static object? GetFieldValue(object? instance, string fieldName)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance), "Переданный объект null.");
        }

        var fields = instance.GetType()
            .GetFields(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance);

        object? value = null;
        var foundField = false;
        foreach (var field in fields)
        {
            if (field.Name == fieldName)
            {
                value = field.GetValue(instance);
                foundField = true;
                break;
            }
        }

        if (!foundField)
        {
            throw new MissingFieldException($"Поле с именем {fieldName} у типа {instance.GetType().Name} не найдено");
        }

        return value;
    }

    public static void SetFieldValue(
        object? instance,
        object? value,
        string fieldName)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance), "Переданный объект null.");
        }

        var fields = GetFields(instance.GetType());
        foreach (var field in fields)
        {
            if (field.Name == fieldName)
            {
                field.SetValue(instance, value);
                break;
            }
        }
    }

    public static MethodInfo GetMethodFromType(
        Type type,
        string methodName,
        Type[] methodPassingTypes)
    {
        var methodInfo = type.GetMethod(methodName, methodPassingTypes);
        if (methodInfo is null)
        {
            throw new MissingMethodException($"Метод {methodName} отсутствует в классе {type.Name}.");
        }

        return methodInfo;
    }

    public static void UpdateFields(
        object source,
        object target,
        IList<ValueChange>? changes = null)
    {
        var fields = GetFields(source.GetType());
        foreach (var field in fields)
        {
            FieldInfo? fieldInTarget;
            try
            {
                fieldInTarget = FindField(target, field.Name);
            }
            catch (ArgumentNullException)
            {
                continue;
            }

            if (fieldInTarget is null)
            {
                continue;
            }

            if (field.GetType() != fieldInTarget.GetType())
            {
                continue;
            }

            var previousValue = GetFieldValue(target, field.Name);
            var newValue = GetFieldValue(source, field.Name);
            if (previousValue is not null && previousValue.Equals(newValue))
            {
                continue;
            }

            SetFieldValue(
                target,
                newValue,
                field.Name);
            changes?.Add(
                new ValueChange(
                    field.Name,
                    previousValue,
                    newValue));
        }
    }

    public static void UpdateProperties(
        object source,
        object target,
        IList<ValueChange>? changes = null)
    {
        var properties = GetProperties(source.GetType());
        foreach (var property in properties)
        {
            PropertyInfo? propertyInTarget;
            try
            {
                propertyInTarget = FindProperty(target, property.Name);
            }
            catch (ArgumentNullException)
            {
                continue;
            }

            if (propertyInTarget is null)
            {
                continue;
            }

            if (property.GetType() != propertyInTarget.GetType())
            {
                continue;
            }

            var previousValue = GetPropertyValue(target, property.Name);
            var newValue = GetPropertyValue(source, property.Name);
            if (previousValue is not null && previousValue.Equals(newValue))
            {
                continue;
            }

            SetPropertyValue(
                target,
                newValue,
                property.Name);
            changes?.Add(
                new ValueChange(
                    property.Name,
                    previousValue,
                    newValue));
        }
    }

    public static void FindPropertyDifference(
        object firstObj,
        object secondObj,
        IList<ValueChange> differences)
    {
        var properties = GetProperties(firstObj.GetType());
        foreach (var property in properties)
        {
            PropertyInfo? propertyInTarget;
            try
            {
                propertyInTarget = FindProperty(secondObj, property.Name);
            }
            catch (ArgumentNullException)
            {
                continue;
            }

            if (propertyInTarget is null)
            {
                continue;
            }

            if (property.GetType() != propertyInTarget.GetType())
            {
                continue;
            }

            var previousValue = GetPropertyValue(secondObj, property.Name);
            var newValue = GetPropertyValue(firstObj, property.Name);
            if (previousValue is not null && previousValue.Equals(newValue))
            {
                continue;
            }

            differences?.Add(
                new ValueChange(
                    property.Name,
                    previousValue,
                    newValue));
        }
    }

    public static IList<ValueChange> FindPropertyDifferencesBetweenObjectsOfTheSameType<T>(T obj1, T obj2)
    {
        var changes = new List<ValueChange>();
        var properties = GetProperties(obj1!.GetType());
        foreach (var property in properties)
        {
            var obj1Value = property.GetValue(obj1);
            var obj2Value = property.GetValue(obj2);
            if (!Equals(obj1Value, obj2Value))
            {
                changes.Add(
                    new ValueChange(
                        property.Name,
                        obj1Value,
                        obj2Value));
            }
        }

        return changes;
    }

    public static void FindFieldDifference(
        object firstObj,
        object secondObj,
        IList<ValueChange> differences)
    {
        var fields = GetFields(firstObj.GetType());
        foreach (var field in fields)
        {
            FieldInfo? fieldInTarget;
            try
            {
                fieldInTarget = FindField(secondObj, field.Name);
            }
            catch (ArgumentNullException)
            {
                continue;
            }

            if (fieldInTarget is null)
            {
                continue;
            }

            if (field.GetType() != fieldInTarget.GetType())
            {
                continue;
            }

            var previousValue = GetFieldValue(secondObj, field.Name);
            var newValue = GetFieldValue(firstObj, field.Name);
            if (previousValue is not null && previousValue.Equals(newValue))
            {
                continue;
            }

            differences.Add(
                new ValueChange(
                    field.Name,
                    previousValue,
                    newValue));
        }
    }

    public static IList<ValueChange> FindDifferencesBetweenObjects(
        object? firstObj,
        object? secondObj,
        bool checkFields = false,
        bool checkProperties = true)
    {
        if (firstObj is null || secondObj is null)
        {
            return Array.Empty<ValueChange>();
        }

        var changes = new List<ValueChange>();
        if (checkFields)
        {
            FindFieldDifference(
                firstObj,
                secondObj,
                changes);
        }

        if (checkProperties)
        {
            FindPropertyDifference(
                firstObj,
                secondObj,
                changes);
        }

        return changes;
    }

    public static IList<ValueChange> UpdateObjectInstanceBasedOnOtherTypeValues(
        object? source,
        object? target,
        bool changeFields = false,
        bool changeProperties = true)
    {
        if (source is null || target is null)
        {
            return Array.Empty<ValueChange>();
        }

        var changes = new List<ValueChange>();
        if (changeFields)
        {
            UpdateFields(
                source,
                target,
                changes);
        }

        if (changeProperties)
        {
            UpdateProperties(
                source,
                target,
                changes);
        }

        return changes;
    }

    public static TTarget Convert<TSource, TTarget>(TSource source)
    {
        var target = Activator.CreateInstance<TTarget>();
        UpdateObjectInstanceBasedOnOtherTypeValues(source, target);
        return target;
    }
}