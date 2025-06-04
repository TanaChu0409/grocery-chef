namespace GroceryChef.Api.Services.Sorting;

public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions)
{
    public SortMapping[] GetMappings<TSource, TDestination>()
        where TSource : class
        where TDestination : class
    {
        SortMappingDefinition<TSource, TDestination>? sortMappingDefination = sortMappingDefinitions
            .OfType<SortMappingDefinition<TSource, TDestination>>()
            .FirstOrDefault();

        if (sortMappingDefination is null)
        {
            throw new InvalidOperationException(
               $"Sort mapping from '{typeof(TSource).Name}' into {typeof(TDestination).Name} isn't defined.");
        }

        return sortMappingDefination.Mappings;
    }

    public bool ValidateMappings<TSource, TDestination>(string? sort)
        where TSource : class
        where TDestination : class
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return true;
        }

       var sortFields = sort
            .Split(',')
            .Select(f => f.Trim().Split(' ')[0])
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .ToList();

        SortMapping[] mapping = GetMappings<TSource, TDestination>();

        return sortFields
            .All(f =>
                mapping.Any(m =>
                    m.SortField.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}
