using GroceryChef.Api.DTOs.Recipes;

namespace GroceryChef.Api.Entities;

internal static class RecipeUnitExtension
{
    public static string GetUnitName(this RecipeUnit unit) =>
        unit switch
        {
            RecipeUnit.Cup => "杯",
            RecipeUnit.Tablespoon => "大匙",
            RecipeUnit.Teaspoon => "小匙",
            RecipeUnit.Ounce => "盎司",
            RecipeUnit.Pound => "磅",
            RecipeUnit.Gram => "公克",
            RecipeUnit.Kilogram => "公斤",
            RecipeUnit.Liter => "公升",
            RecipeUnit.Milliliter => "毫升",
            RecipeUnit.AppropriateAmount => "適量",
            RecipeUnit.Stalk => "根",
            RecipeUnit.Bag => "包",
            _ => string.Empty
        };

    public static List<RecipeUnitDto> GetOptions() =>
        Enum.GetValues<RecipeUnit>().Select(x => new RecipeUnitDto
        {
            Key = (int)x,
            Value = x.GetUnitName()
        })
        .ToList();
}
