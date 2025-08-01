using System.ComponentModel.DataAnnotations;

namespace GroceryChefClient.UI.Dtos.Carts;

public sealed class EditCartViewModel
{
    public string Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }

    public EditCartDto ToDto() =>
        new()
        {
            Id = Id,
            Name = Name
        };
}

