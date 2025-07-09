namespace GroceryChefClient.UI.Dtos.Ingredients;

public sealed class IngredientViewModel
{
    public string? Id { get; set; }
    public string Name { get; set; }
    public int ShelfLifeOfDate { get; set; }
    public bool IsAllergy { get; set; }
    public bool IsNotAllergy { get; set; }

    public UpdateIngredientDto ToDto() =>
        new()
        {
            Name = Name,
            ShelfLifeOfDate = ShelfLifeOfDate,
            IsAllergy = IsAllergy && !IsNotAllergy
        };
}
