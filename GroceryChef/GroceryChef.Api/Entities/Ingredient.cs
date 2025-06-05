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
    public int ShelfLifeOfDate { get; private set; }
    public bool IsAllergy { get; private set; }
    public DateTime CreateAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static Ingredient Create(
        string name,
        int shelfLifeOfDate,
        bool isAllergy,
        DateTime createAtUtc)
    {
        return new Ingredient
        {
            Id = $"I_{Ulid.NewUlid()}",
            Name = name,
            ShelfLifeOfDate = shelfLifeOfDate,
            IsAllergy = isAllergy,
            CreateAtUtc = createAtUtc,
        };
    }

    public void UpdateFromDto(UpdateIngredientDto updateIngredient)
    {
        Name = updateIngredient.Name;
        ShelfLifeOfDate = updateIngredient.ShelfLifeOfDate;
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
            ShelfLifeOfDate = ShelfLifeOfDate,
            IsAllergy = IsAllergy,
            CreatedAtUtc = CreateAtUtc,
            UpdatedAtUtc = UpdatedAtUtc,
            Links = []
        };

    public static readonly SortMappingDefinition<IngredientDto, Ingredient> SortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(IngredientDto.Name), nameof(Name)),
            new SortMapping(nameof(IngredientDto.ShelfLifeOfDate), nameof(ShelfLifeOfDate)),
            new SortMapping(nameof(IngredientDto.CreatedAtUtc), nameof(CreateAtUtc)),
            new SortMapping(nameof(IngredientDto.UpdatedAtUtc), nameof(UpdatedAtUtc))
        ]
    };
}
