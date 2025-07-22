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
}
