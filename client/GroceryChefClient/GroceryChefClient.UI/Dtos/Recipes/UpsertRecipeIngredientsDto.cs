﻿namespace GroceryChefClient.UI.Dtos.Recipes;

public sealed record UpsertRecipeIngredientsDto
{
    public List<UpsertRecipeIngredientDetailDto> Details { get; init; }
}

public sealed record UpsertRecipeIngredientDetailDto
{
    public required string IngredientId { get; init; } = string.Empty;
    public required decimal Amount { get; init; }
    public required int Unit { get; init; }
}
