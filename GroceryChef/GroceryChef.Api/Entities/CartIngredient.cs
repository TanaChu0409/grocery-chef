namespace GroceryChef.Api.Entities;

public sealed class CartIngredient
{
    public string CartId { get; set; }
    public string IngrdientId { get; set; }
    public int Quantity { get; set; }
    public bool IsBought { get; set; }
}
