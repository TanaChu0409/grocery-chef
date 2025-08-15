namespace GroceryChefClient.UI.Dtos.Carts;

public sealed class CartDetailGridViewModel
{
    public string IngredientId { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public bool IsBought { get; set; }
}
