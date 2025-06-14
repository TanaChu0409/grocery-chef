﻿using GroceryChef.Api.DTOs.Abstructions;
using GroceryChef.Api.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace GroceryChef.Api.DTOs.Ingredients;

public sealed record IngredientQueryParameters : BaseQueryParameters
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
    public bool? IsAllergy { get; init; }
    public int? ShelfLifeOfDate { get; init; }
}
