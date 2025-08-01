using System.ComponentModel.DataAnnotations;

namespace GroceryChefClient.UI.Dtos.Carts;

public sealed class CreateCartViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }

    public CreateCartDto ToDto() =>
        new()
        {
            Name = Name
        };
}

public sealed record CreateCartDto
{
    public required string Name { get; init; }
}

