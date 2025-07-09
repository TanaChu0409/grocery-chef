namespace GroceryChefClient.UI.Dtos.Ingredients;

public sealed record IngredientDto
{
    public required string Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required int ShelfLifeOfDate { get; init; }
    public required bool IsAllergy { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }

    public IngredientViewModel ToViewModel() =>
        new()
        {
            Id = Id,
            Name = Name,
            ShelfLifeOfDate = ShelfLifeOfDate,
            IsAllergy = IsAllergy,
            IsNotAllergy = !IsAllergy
        };
}
