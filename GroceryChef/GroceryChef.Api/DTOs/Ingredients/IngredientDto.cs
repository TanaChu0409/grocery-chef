namespace GroceryChef.Api.DTOs.Ingredients;

public sealed record IngredientDto
{
    public required string Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required DateTime ShelfLife { get; init; }
    public required bool IsAllergy { get; init; }
    public required DateTime CreateAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
