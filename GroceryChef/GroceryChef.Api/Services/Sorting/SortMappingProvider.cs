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
}
