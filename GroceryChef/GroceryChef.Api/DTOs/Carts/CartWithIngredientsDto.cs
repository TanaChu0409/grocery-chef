namespace GroceryChef.Api.DTOs.Carts;

public sealed record CartWithIngredientsDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public required List<CartIngredientsDetailDto> Ingredients { get; set; } = [];
}


public sealed record CartIngredientsDetailDto
{
    public required string IngredientId { get; init; }
    public required string Name { get; init; }
    public required bool IsBought { get; init; }
}
