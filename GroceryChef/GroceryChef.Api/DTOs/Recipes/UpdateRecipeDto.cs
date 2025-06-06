namespace GroceryChef.Api.DTOs.Recipes;

public sealed record UpdateRecipeDto
{
    public required string Name { get; init; } = string.Empty;
    public required string Content { get; init; } = string.Empty;
    public string? Descriptions { get; init; }
}
