namespace GroceryChef.Api.DTOs.Carts;

public sealed record UpdateCartDto
{
    public required string Name { get; init; } = string.Empty;
}
