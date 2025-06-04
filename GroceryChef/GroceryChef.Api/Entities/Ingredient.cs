using System.Linq.Expressions;
using GroceryChef.Api.DTOs.Ingredients;
using GroceryChef.Api.Services.Sorting;

namespace GroceryChef.Api.Entities;

public sealed class Ingredient
{
    private Ingredient()
    {
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    public DateOnly ShelfLife { get; private set; }
    public bool IsAllergy { get; private set; }
    public DateTime CreateAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static Ingredient Create(
        string name,
        DateOnly shelfLife,
        bool isAllergy,
        DateTime createAtUtc)
    {
        return new Ingredient
        {
            Id = $"I_{Ulid.NewUlid()}",
            Name = name,
            ShelfLife = shelfLife,
            IsAllergy = isAllergy,
            CreateAtUtc = createAtUtc,
        };
    }

    public void UpdateFromDto(UpdateIngredientDto updateIngredient)
    {
        Name = updateIngredient.Name;
        ShelfLife = updateIngredient.ShelfLife;
        IsAllergy = updateIngredient.IsAllergy;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Expression<Func<Ingredient, IngredientDto>> ProjectToDto() =>
        i => i.ToDto();

    public IngredientDto ToDto() =>
        new()
        {
            Id = Id,
            Name = Name,
            ShelfLife = ShelfLife,
            IsAllergy = IsAllergy,
            CreateAtUtc = CreateAtUtc,
            UpdatedAtUtc = UpdatedAtUtc
        };

    public static readonly SortMappingDefinition<IngredientDto, Ingredient> SortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(IngredientDto.Name), nameof(Name)),
            new SortMapping(nameof(IngredientDto.ShelfLife), nameof(ShelfLife)),
            new SortMapping(nameof(IngredientDto.CreateAtUtc), nameof(CreateAtUtc)),
            new SortMapping(nameof(IngredientDto.UpdatedAtUtc), nameof(UpdatedAtUtc))
        ]
    };
}
