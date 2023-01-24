using System.Reflection;

namespace StudyOrganizer.Loaders;

public static class AssemblyLoader
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

    public static object? GetStaticFieldValue(Type type, string fieldName)
    {
        var fields = type.GetFields(BindingFlags.Public | 
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Instance);
        
        object? value = null;
        bool foundField = false;
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
            throw new MissingFieldException(
                $"Поле с именем {fieldName} у типа {type.Name} не найдено");
        }

        return value;
    }
    
    public static object? GetFieldValue(object? instance, string fieldName)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance), "Переданный объект null.");
        }

        var fields = instance.GetType().GetFields(BindingFlags.Public | 
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Instance);

        object? value = null;
        bool foundField = false;
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
            throw new MissingFieldException(
                $"Поле с именем {fieldName} у типа {instance.GetType().Name} не найдено");
        }

        return value;
    } 
    
    public static MethodInfo GetMethodFromType(
        Type type, 
        string methodName,
        Type[] methodPassingTypes)
    {
        var methodInfo = type.GetMethod(methodName, methodPassingTypes);
        if (methodInfo is null)
        {
            throw new MissingMethodException(
                $"Метод {methodName} отсутствует в классе {type.Name}.");
        }

        return methodInfo;
    }
}