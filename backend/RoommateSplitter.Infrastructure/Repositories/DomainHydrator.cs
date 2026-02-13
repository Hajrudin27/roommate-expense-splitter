using System.Reflection;
using System.Runtime.Serialization;

namespace RoommateSplitter.Infrastructure.Repositories;

internal static class DomainHydrator
{
    internal static T Create<T>() where T : class
    {
        return (T)FormatterServices.GetUninitializedObject(typeof(T));
    }

    internal static T Set<T>(T obj, string propertyName, object? value)
    {
        var type = typeof(T);

        var prop = type.GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        if (prop is null)
            throw new InvalidOperationException($"Property '{propertyName}' not found on {type.Name}.");

        var setter = prop.SetMethod;
        if (setter is not null)
        {
            prop.SetValue(obj, value);
            return obj;
        }

        var backingFieldName = $"<{propertyName}>k__BackingField";
        var backingField = type.GetField(
            backingFieldName,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        if (backingField is not null)
        {
            backingField.SetValue(obj, value);
            return obj;
        }

        var candidates = new[]
        {
            "_" + char.ToLowerInvariant(propertyName[0]) + propertyName[1..],
            "_" + propertyName,
            char.ToLowerInvariant(propertyName[0]) + propertyName[1..],
            propertyName
        };

        foreach (var fieldName in candidates)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null) continue;

            field.SetValue(obj, value);
            return obj;
        }

        throw new InvalidOperationException(
            $"Property '{propertyName}' on {type.Name} has no setter and no matching backing field was found."
        );
    }

    internal static void SetField<T>(T obj, string fieldName, object? value)
    {
        var field = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
            throw new InvalidOperationException($"Field '{fieldName}' not found on {typeof(T).Name}.");

        field.SetValue(obj, value);
    }
}
