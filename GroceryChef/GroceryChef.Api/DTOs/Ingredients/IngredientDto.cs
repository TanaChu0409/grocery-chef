using GroceryChef.Api.DTOs.Common;

namespace GroceryChef.Api.DTOs.Ingredients;

public sealed record IngredientDto : ILinkResponse
{
    public required string Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required DateOnly ShelfLife { get; init; }
    public required bool IsAllergy { get; init; }
    public required DateTime CreateAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public List<LinkDto> Links { get; set; }
}
