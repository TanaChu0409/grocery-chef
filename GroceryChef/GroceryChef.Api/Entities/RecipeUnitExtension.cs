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
            _ => string.Empty
        };

    public static Dictionary<int, string> GetOptions() =>
        Enum.GetValues<RecipeUnit>()
            .ToDictionary(unit => (int)unit, unit => unit.GetUnitName());
}
