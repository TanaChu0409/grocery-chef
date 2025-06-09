using GroceryChef.Api.DTOs.Common;

namespace GroceryChef.Api.DTOs.Carts;

public sealed record CartDto : ILinkResponse
{
    public required string Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public List<LinkDto> Links { get; set; }
}
