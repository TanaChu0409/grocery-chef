namespace GroceryChefClient.UI.Dtos.Carts;

public sealed record EditCartDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }

    public EditCartViewModel ToViewModel() =>
        new()
        {
            Id = Id,
            Name = Name
        };
}

