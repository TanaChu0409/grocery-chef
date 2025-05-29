namespace GroceryChef.Api.Services.Sorting;

public sealed class SortMappingDefinition<TSource, TDestination> : ISortMappingDefinition
    where TSource : class
    where TDestination : class
{
    public SortMapping[] Mappings { get; init; }
}
