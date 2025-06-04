namespace GroceryChef.Api.DTOs.Ingredients;

public sealed record CreateIngredientDto
{
    public required string Name { get; init; } = string.Empty;
    public required int ShelfLifeOfDate { get; init; }
    public required bool IsAllergy { get; init; }
}
