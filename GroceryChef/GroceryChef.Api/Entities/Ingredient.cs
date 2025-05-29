using System.Linq.Expressions;
using GroceryChef.Api.DTOs.Ingredients;

namespace GroceryChef.Api.Entities;

public sealed class Ingredient
{
    private Ingredient()
    {
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    public DateTime ShelfLife { get; private set; }
    public bool IsAllergy { get; private set; }
    public DateTime CreateAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static Ingredient Create(
        string name,
        DateTime shelfLife,
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
}
