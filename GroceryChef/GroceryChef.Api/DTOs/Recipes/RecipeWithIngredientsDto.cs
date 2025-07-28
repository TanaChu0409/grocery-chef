namespace GroceryChef.Api.DTOs.Recipes;

public sealed record RecipeWithIngredientsDto
{
    public required string Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required string Content { get; init; } = string.Empty;
    public string? Description { get; init; }
    public required bool IsArchived { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public required string[] IngredientsWithUnit { get; init; }
    public required List<RecipeIngredientDetail> RecipeIngredientDetails { get; init; }
}

public sealed record RecipeIngredientDetail
{
    public required string IngredientId { get; init; }
    public required string IngredientName { get; init; } = string.Empty;
    public required decimal Amount { get; init; }
    public required int Unit { get; init; }
    public required string UnitName { get; init; } = string.Empty;
}
