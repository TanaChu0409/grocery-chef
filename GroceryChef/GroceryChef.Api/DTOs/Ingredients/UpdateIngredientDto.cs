namespace GroceryChef.Api.DTOs.Ingredients;

public sealed record UpdateIngredientDto
{
    public required string Name { get; init; } = string.Empty;
    public required DateOnly ShelfLife { get; init; }
    public required bool IsAllergy { get; init; }
}
