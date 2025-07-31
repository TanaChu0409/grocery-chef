namespace GroceryChefClient.UI.Dtos.Carts;

public sealed record CartDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }

    public CartViewModel ToViewModel() =>
        new()
        {
            Id = Id,
            Name = Name,
            CreateAtUtc = CreatedAtUtc,
            UpdatedAtUtc = UpdatedAtUtc
        };
}
