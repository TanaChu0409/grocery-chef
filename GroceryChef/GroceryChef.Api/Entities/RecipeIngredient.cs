namespace GroceryChef.Api.Entities;

public sealed class RecipeIngredient
{
    public string RecipeId { get; set; }
    public string IngredientId { get; set; }
    public decimal Amount { get; set; }
    public RecipeUnit Unit { get; set; }
    public DateTime CreateAtUtc { get; set; }
}
