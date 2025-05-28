namespace GroceryChef.Api.Entities;

public sealed class RecipeIngredient
{
    public string RecipeId { get; set; }
    public string Ingredient { get; set; }
    public DateTime CreateAtUtc { get; set; }
}
