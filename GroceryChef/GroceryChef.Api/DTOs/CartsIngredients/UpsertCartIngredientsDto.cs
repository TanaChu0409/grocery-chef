namespace GroceryChef.Api.DTOs.CartsIngredients;

public sealed record UpsertCartIngredientsDto
{
    public required List<UpsertCartIngredientsDetailDto> Details { get; init; } = [];
}

public sealed record UpsertCartIngredientsDetailDto
{
    public required string IngredientId { get; init; } = string.Empty;
    public required int Quantity { get; init; }
}
