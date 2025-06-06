namespace GroceryChef.Api.Entities;

public enum RecipeUnit
{
    None = 0,
    Cup = 1,
    Tablespoon = 2,
    Teaspoon = 3,
    Ounce = 4,
    Pound = 5,
    Gram = 6,
    Kilogram = 7,
    Liter = 8,
    Milliliter = 9
}

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
            _ => "無單位"
        };
}
