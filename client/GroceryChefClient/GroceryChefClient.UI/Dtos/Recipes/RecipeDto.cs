﻿namespace GroceryChefClient.UI.Dtos.Recipes;

public sealed record RecipeDto
{
    private const string IngredientSeparator = "\n";
    public required string Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required string Content { get; init; } = string.Empty;
    public string? Description { get; init; }
    public required bool IsArchived { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public string[] IngredientsWithUnit { get; init; }
    public List<RecipeIngredientDetail> RecipeIngredientDetails { get; init; }

    public RecipeViewModel ToViewModel() =>
        new()
        {
            Id = Id,
            Name = Name,
            Content = Content,
            Description = Description,
            IngredientsWithUnitDisplay = string.Join(IngredientSeparator, IngredientsWithUnit) ?? string.Empty
        };
}

public sealed record RecipeIngredientDetail
{
    public required string IngredientId { get; init; }
    public required string IngredientName { get; init; } = string.Empty;
    public required decimal Amount { get; init; }
    public required int Unit { get; init; }
    public required string UnitName { get; init; }

    public RecipeIngredientDetailViewModel ToViewModel() =>
        new()
        {
            IngredientId = IngredientId,
            IngredientName = IngredientName,
            Amount = Amount,
            Unit = Unit,
            UnitName = UnitName
        };
}
