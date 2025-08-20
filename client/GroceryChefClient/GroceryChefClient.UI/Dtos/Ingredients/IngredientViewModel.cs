using System.ComponentModel.DataAnnotations;

namespace GroceryChefClient.UI.Dtos.Ingredients;

public sealed class IngredientViewModel
{
    public string? Id { get; set; }
    [Required(ErrorMessage = "Ingredient name is required.")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Ingredient shelf life of date is required.")]
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
