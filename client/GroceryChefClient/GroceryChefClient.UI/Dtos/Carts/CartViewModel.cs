namespace GroceryChefClient.UI.Dtos.Carts;

public sealed class CartViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime CreateAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
