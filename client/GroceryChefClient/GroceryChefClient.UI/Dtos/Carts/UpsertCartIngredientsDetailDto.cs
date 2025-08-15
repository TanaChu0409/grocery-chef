namespace GroceryChefClient.UI.Dtos.Carts;

public sealed record UpsertCartIngredientsDetailDto
{
    public required string IngredientId { get; init; } = string.Empty;
    public required int Quantity { get; init; }
}
