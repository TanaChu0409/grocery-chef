using System.ComponentModel.DataAnnotations;

namespace GroceryChefClient.UI.Dtos.Carts;

public sealed class CartDetailViewModel
{
    [Required(ErrorMessage = "You must choose a cart to view details.")]
    public string CartId { get; set; }

    public List<CartDetailOptions> Options { get; set; } = new();
}

public sealed class CartDetailOptions
{
    public string? CartId { get; set; }
    public string? CartName { get; set; }
}
