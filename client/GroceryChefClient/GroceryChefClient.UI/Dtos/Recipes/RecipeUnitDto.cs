namespace GroceryChefClient.UI.Dtos.Recipes;

public sealed record RecipeUnitDto
{
    public required int Key { get; init; }
    public required string Value { get; init; }
}
