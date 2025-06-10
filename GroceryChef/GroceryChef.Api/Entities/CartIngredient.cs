namespace GroceryChef.Api.Entities;

public sealed class CartIngredient
{
    public string CartId { get; set; }
    public string IngredientId { get; set; }
    public int Quantity { get; set; }
    public bool IsBought { get; set; }
}
