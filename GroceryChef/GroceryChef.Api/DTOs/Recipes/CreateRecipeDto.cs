namespace GroceryChef.Api.DTOs.Recipes;

public sealed record CreateRecipeDto
{
    public required string Name { get; init; } = string.Empty;
    public required string Content { get; init; } = string.Empty;
    public string? Description { get; init; }
}
