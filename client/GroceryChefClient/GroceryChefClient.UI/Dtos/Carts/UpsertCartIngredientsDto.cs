namespace GroceryChefClient.UI.Dtos.Carts;

public sealed record UpsertCartIngredientsDto
{
    public required List<UpsertCartIngredientsDetailDto> Details { get; init; } = [];
}
