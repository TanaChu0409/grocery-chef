using GroceryChef.Api.DTOs.Common;

namespace GroceryChef.Api.DTOs.Recipes;

public sealed record RecipeDto : ILinkResponse
{
    public required string Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required string Content { get; init; } = string.Empty;
    public string? Descriptions { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public List<LinkDto> Links { get; set; }
}
