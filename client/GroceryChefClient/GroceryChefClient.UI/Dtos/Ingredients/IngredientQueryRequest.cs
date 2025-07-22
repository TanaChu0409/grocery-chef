using BlazorBootstrap;
using GroceryChefClient.UI.Dtos.Common;

namespace GroceryChefClient.UI.Dtos.Ingredients;

public sealed record IngredientQueryRequest : BaseQueryRequest
{
    public bool? IsAllergy =>
        Filters.Any(f => f.PropertyName == nameof(IsAllergy)) ?
        Filters.FirstOrDefault(f => f.PropertyName == nameof(IsAllergy))?.Value.ToLowerInvariant() == "true" :
        null;
    public int? ShelfLifeOfDate =>
        Filters?.FirstOrDefault(f => f.PropertyName == nameof(ShelfLifeOfDate))?.Value is string value &&
        int.TryParse(value, out int shelfLife) ?
            shelfLife :
        null;
}
