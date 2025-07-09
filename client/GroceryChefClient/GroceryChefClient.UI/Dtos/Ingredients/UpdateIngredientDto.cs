namespace GroceryChefClient.UI.Dtos.Ingredients;

public sealed record UpdateIngredientDto
{
    public required string Name { get; init; } = string.Empty;
    public required int ShelfLifeOfDate { get; init; }
    public required bool IsAllergy { get; init; }
}
