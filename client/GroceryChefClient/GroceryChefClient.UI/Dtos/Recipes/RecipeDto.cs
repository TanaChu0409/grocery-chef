namespace GroceryChefClient.UI.Dtos.Recipes;

public sealed record RecipeDto
{
    public required string Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required string Content { get; init; } = string.Empty;
    public string? Description { get; init; }
    public required bool IsArchived { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
