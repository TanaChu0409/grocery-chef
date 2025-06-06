using GroceryChef.Api.DTOs.Abstructions;
using Microsoft.AspNetCore.Mvc;

namespace GroceryChef.Api.DTOs.Recipes;

public sealed record RecipeQueryParameters : BaseQueryParameters
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
}
