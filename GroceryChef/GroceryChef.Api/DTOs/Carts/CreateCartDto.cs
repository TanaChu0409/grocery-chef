namespace GroceryChef.Api.DTOs.Carts;

public sealed record CreateCartDto
{
    public required string Name { get; init; } = string.Empty;
}
