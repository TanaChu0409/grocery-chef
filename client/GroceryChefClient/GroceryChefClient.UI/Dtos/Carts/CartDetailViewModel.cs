using System.ComponentModel.DataAnnotations;

namespace GroceryChefClient.UI.Dtos.Carts;

public sealed class CartDetailViewModel
{
    public string? CartId { get; set; }

    public List<CartDetailOptions> Options { get; set; } = [];
}

public sealed class CartDetailOptions
{
    public string? CartId { get; set; }
    public string? CartName { get; set; }
}
