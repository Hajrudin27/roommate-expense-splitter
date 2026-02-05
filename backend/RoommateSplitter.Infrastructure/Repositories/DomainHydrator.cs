using System.Reflection;

namespace RoommateSplitter.Infrastructure.Repositories;

internal static class DomainHydrator
{
    // Sets private-set or private fields for aggregate hydration from persistence.
    internal static T Set<T>(T obj, string propertyName, object? value)
    {
        var prop = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop is null)
            throw new InvalidOperationException($"Property '{propertyName}' not found on {typeof(T).Name}.");

        prop.SetValue(obj, value);
        return obj;
    }

    internal static void SetField<T>(T obj, string fieldName, object? value)
    {
        var field = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
            throw new InvalidOperationException($"Field '{fieldName}' not found on {typeof(T).Name}.");

        field.SetValue(obj, value);
    }

    internal static T Create<T>()
        where T : class
    {
        return (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;
    }
}
