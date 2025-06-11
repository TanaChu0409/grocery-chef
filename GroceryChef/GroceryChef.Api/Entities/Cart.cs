using System.Linq.Expressions;
using GroceryChef.Api.DTOs.Carts;
using GroceryChef.Api.DTOs.CartsIngredients;
using GroceryChef.Api.Services.Sorting;

namespace GroceryChef.Api.Entities;

public sealed class Cart
{
    private readonly List<Ingredient> _ingredients = [];
    private readonly List<CartIngredient> _cartIngredients = [];

    private Cart()
    {
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients;
    public IReadOnlyCollection<CartIngredient> CartIngredients => _cartIngredients;

    public static Cart Create(string name, DateTime createdAtUtc)
    {
        return new Cart
        {
            Id = $"c_{Ulid.NewUlid()}",
            Name = name,
            CreatedAtUtc = createdAtUtc
        };
    }

    public void UpdateFromDto(UpdateCartDto updateCart, DateTime updatedAtUtc)
    {
        Name = updateCart.Name;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void RemoveAllCartIngredients(List<string> upsertCartIngredientIds)
    {
        _cartIngredients.RemoveAll(ci => !upsertCartIngredientIds.Contains(ci.IngredientId));
    }

    public void AddCartIngredients(List<UpsertCartIngredientsDetailDto> upsertCartIngredientDetails)
    {
        _cartIngredients.AddRange(
            upsertCartIngredientDetails
            .Select(uci =>
            new CartIngredient
            {
                CartId = Id,
                IngredientId = uci.IngredientId,
                Quantity = uci.Quantity,
                IsBought = false
            }));
    }

    public static Expression<Func<Cart, CartDto>> ProjectToDto() =>
        c => c.ToDto();

    public static Expression<Func<Cart, CartWithIngredientsDto>> ProjectToDtoWithIngredients() =>
        c => c.ToDtoWithIngredients();

    public CartDto ToDto() =>
        new()
        {
            Id = Id,
            Name = Name,
            CreatedAtUtc = CreatedAtUtc,
            UpdatedAtUtc = UpdatedAtUtc,
        };

    public CartWithIngredientsDto ToDtoWithIngredients() =>
        new()
        {
            Id = Id,
            Name = Name,
            CreatedAtUtc = CreatedAtUtc,
            UpdatedAtUtc = UpdatedAtUtc,
            Ingredients = _cartIngredients
                .ToDictionary(ci => GetIngredientName(ci.IngredientId), ci => ci.IsBought)
        };

    private string GetIngredientName(string ingredientId) =>
        _ingredients.FirstOrDefault(i => i.Id == ingredientId)?.Name ?? string.Empty;

    public static readonly SortMappingDefinition<CartDto, Cart> SortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(CartDto.Name), nameof(Name)),
            new SortMapping(nameof(CartDto.CreatedAtUtc), nameof(CreatedAtUtc)),
            new SortMapping(nameof(CartDto.UpdatedAtUtc), nameof(UpdatedAtUtc)),
        ]
    };
}
