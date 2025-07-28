using System.ComponentModel.DataAnnotations;

namespace GroceryChefClient.UI.Dtos.Recipes;

public sealed class RecipeViewModel
{
    public string? Id { get; set; }
    [Required(ErrorMessage = "Recipe name is required.")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Recipe content is required.")]
    public string Content { get; set; }
    public string? Description { get; set; }
    public string IngredientsWithUnitDisplay { get; set; }

    public UpdateRecipeDto ToDto() =>
        new()
        {
            Name = Name,
            Content = Content,
            Description = Description
        };
}

public sealed class RecipeIngredientDetailViewModel
{
    public string IngredientId { get; set; }
    public string IngredientName { get; set; }
    public decimal Amount { get; set; }
    public int Unit { get; set; }
    public string UnitName { get; set; }
    public RecipeIngredientDetail ToDto() =>
        new()
        {
            IngredientId = IngredientId,
            IngredientName = IngredientName,
            Amount = Amount,
            Unit = Unit,
            UnitName = UnitName
        };
}
