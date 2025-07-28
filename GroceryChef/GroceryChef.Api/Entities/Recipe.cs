using System;
using System.Linq.Expressions;
using GroceryChef.Api.DTOs.RecipeIngredients;
using GroceryChef.Api.DTOs.Recipes;
using GroceryChef.Api.Services.Sorting;

namespace GroceryChef.Api.Entities;

public sealed class Recipe
{
    private readonly List<RecipeIngredient> _recipeIngredients = [];
    private readonly List<Ingredient> _ingredients = [];

    private Recipe()
    {
    }

    public string Id { get; private set; }
    public string UserId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Content { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<RecipeIngredient> RecipeIngredients => _recipeIngredients;
    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients;

    public static Recipe Create(
        string name,
        string? descriptions,
        string content,
        string userId,
        DateTime createAtUtc)
    {
        return new Recipe
        {
            Id = $"R_{Ulid.NewUlid()}",
            Name = name,
            Content = content,
            Description = descriptions,
            CreatedAtUtc = createAtUtc,
            IsArchived = false,
            UserId = userId
        };
    }

    public void Archived()
    {
        IsArchived = true;
    }

    public void Restore()
    {
        IsArchived = false;
    }

    public void RemoveAllRecipeIngredients(List<string> upsertRecipeIngredientIds)
    {
        _recipeIngredients.RemoveAll(ri => !upsertRecipeIngredientIds.Contains(ri.IngredientId));
    }

    public void AddRecipeIngredients(
        List<UpsertRecipeIngredientDetailDto> upsertRecipeIngredientDetails,
        DateTime createdAtUtc)
    {
        _recipeIngredients.AddRange(
            upsertRecipeIngredientDetails
            .Select(uri => new RecipeIngredient
            {
                RecipeId = Id,
                IngredientId = uri.IngredientId,
                Amount = uri.Amount,
                Unit = uri.Unit ?? RecipeUnit.None,
                CreateAtUtc = createdAtUtc
            }));
    }

    public void UpdateFromDto(UpdateRecipeDto updateRecipe)
    {
        Name = updateRecipe.Name;
        Content = updateRecipe.Content;
        Description = updateRecipe.Description;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Expression<Func<Recipe, RecipeDto>> ProjectToDto() =>
        r => r.ToDto();

    public static Expression<Func<Recipe, RecipeWithIngredientsDto>> ProjectToDtoWithIngredients() =>
        r => r.ToDtoWithIngredients();
    public RecipeDto ToDto() =>
        new()
        {
            Id = Id,
            Name = Name,
            Content = Content,
            Description = Description,
            IsArchived = IsArchived,
            CreatedAtUtc = CreatedAtUtc,
            UpdatedAtUtc = UpdatedAtUtc,
            Links = []
        };

    public RecipeWithIngredientsDto ToDtoWithIngredients() =>
        new()
        {
            Id = Id,
            Name = Name,
            Content = Content,
            Description = Description,
            IsArchived = IsArchived,
            CreatedAtUtc = CreatedAtUtc,
            UpdatedAtUtc = UpdatedAtUtc,
            IngredientsWithUnit = _recipeIngredients
                .Select(ri => GetIngredientNameWithUnit(
                    ri.IngredientId,
                    ri.Amount,
                    ri.Unit))
                .ToArray(),
            RecipeIngredientDetails = _recipeIngredients
                .Select(GetRecipeIngredientDetails())
                .ToList()
        };

    private Func<RecipeIngredient, RecipeIngredientDetail> GetRecipeIngredientDetails() => 
        ri => new RecipeIngredientDetail
        {
            IngredientId = ri.IngredientId,
            IngredientName = _ingredients.FirstOrDefault(i => i.Id == ri.IngredientId)?.Name ?? string.Empty,
            Amount = ri.Amount,
            Unit = (int)ri.Unit,
            UnitName = RecipeUnitExtension.GetUnitName(ri.Unit)
        };

    private string GetIngredientNameWithUnit(string ingredientId, decimal amount, RecipeUnit unit) =>
        $"{_ingredients.FirstOrDefault(i => i.Id == ingredientId)?.Name} {amount} ({RecipeUnitExtension.GetUnitName(unit)})";

    public static readonly SortMappingDefinition<RecipeDto, Recipe> SortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(RecipeDto.Name), nameof(Name)),
            new SortMapping(nameof(RecipeDto.Content), nameof(Content)),
            new SortMapping(nameof(RecipeDto.Description), nameof(Description)),
            new SortMapping(nameof(RecipeDto.CreatedAtUtc), nameof(CreatedAtUtc)),
            new SortMapping(nameof(RecipeDto.UpdatedAtUtc), nameof(UpdatedAtUtc))
        ]
    };
}
