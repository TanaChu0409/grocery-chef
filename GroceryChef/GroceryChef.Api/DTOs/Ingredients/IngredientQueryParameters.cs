using GroceryChef.Api.DTOs.Abstructions;
using Microsoft.AspNetCore.Mvc;

namespace GroceryChef.Api.DTOs.Ingredients;

public sealed record IngredientQueryParameters : BaseQueryParameters
{
    [FromQuery(Name = "q")]
    public string? Search { get; init; }
}
