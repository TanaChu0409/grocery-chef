using System;
using System.Linq.Expressions;
using GroceryChef.Api.DTOs.Recipes;
using GroceryChef.Api.Services.Sorting;

namespace GroceryChef.Api.Entities;

public sealed class Recipe
{
    private readonly List<Ingredient> _ingredients = [];

    private Recipe()
    {
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Content { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients;

    public static Recipe Create(
        string name,
        string? descriptions,
        string content,
        DateTime createAtUtc)
    {
        return new Recipe
        {
            Id = $"R_{Ulid.NewUlid()}",
            Name = name,
            Content = content,
            Description = descriptions,
            CreatedAtUtc = createAtUtc,
            IsArchived = false
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
    public void UpdateFromDto(UpdateRecipeDto updateRecipe)
    {
        Name = updateRecipe.Name;
        Content = updateRecipe.Content;
        Description = updateRecipe.Descriptions;
        UpdatedAtUtc = DateTime.UtcNow;
    }
    public static Expression<Func<Recipe, RecipeDto>> ProjectToDto() =>
        r => r.ToDto();
    public RecipeDto ToDto() =>
        new()
        {
            Id = Id,
            Name = Name,
            Content = Content,
            Descriptions = Description,
            CreatedAtUtc = CreatedAtUtc,
            UpdatedAtUtc = UpdatedAtUtc,
            Links = []
        };

    public static readonly SortMappingDefinition<RecipeDto, Recipe> SortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(RecipeDto.Name), nameof(Name)),
            new SortMapping(nameof(RecipeDto.Content), nameof(Content)),
            new SortMapping(nameof(RecipeDto.Descriptions), nameof(Description)),
            new SortMapping(nameof(RecipeDto.CreatedAtUtc), nameof(CreatedAtUtc)),
            new SortMapping(nameof(RecipeDto.UpdatedAtUtc), nameof(UpdatedAtUtc))
        ]
    };
}
