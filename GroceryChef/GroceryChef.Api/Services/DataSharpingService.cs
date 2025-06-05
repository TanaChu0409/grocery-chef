using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;
using GroceryChef.Api.DTOs.Common;

namespace GroceryChef.Api.Services;

public sealed class DataSharpingService
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        HashSet<string> fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        PropertyInfo[] propertyInfos = PropertyCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldsSet.Any())
        {
            propertyInfos = propertyInfos
                .Where(p => fieldsSet.Contains(p.Name))
                .ToArray();
        }

        IDictionary<string, object?> sharpObject = new ExpandoObject();

        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            sharpObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
        }

        return (ExpandoObject)sharpObject;
    }

    public List<ExpandoObject> ShapeCollectionData<T>(
        IEnumerable<T> entities,
        string? fields,
        Func<T, List<LinkDto>>? linksFactory = null)
    {
        HashSet<string> fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        PropertyInfo[] propertyInfos = PropertyCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldsSet.Any())
        {
            propertyInfos = propertyInfos
                .Where(p => fieldsSet.Contains(p.Name))
                .ToArray();
        }

        List<ExpandoObject> shapedObjects = [];
        foreach (T entity in entities)
        {
            IDictionary<string, object?> shapeObject = new ExpandoObject();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                shapeObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
            }

            if (linksFactory is not null)
            {
                shapeObject["Links"] = linksFactory(entity);
            }

            shapedObjects.Add((ExpandoObject)shapeObject);
        }

        return shapedObjects;
    }

    public bool Validate<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }

        var fieldsSet = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        PropertyInfo[] propertyInfos = PropertyCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        return fieldsSet.All(f =>
            propertyInfos.Any(p =>
                p.Name.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}
