using GroceryChefClient.UI.Dtos.Common;

namespace GroceryChefClient.UI.Dtos.Ingredients;

public sealed record IngredientQueryRequest : BaseQueryRequest
{
    public bool? IsAllergy { get; init; }
    public int? ShelfLifeOfDate { get; init; }
}