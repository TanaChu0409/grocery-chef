using BlazorBootstrap;

namespace GroceryChefClient.UI.Extensions;

public static class GridExtensions
{
    public static string GetSortDirectionString(this SortDirection sortDirection) =>
        sortDirection switch
        {
            SortDirection.Ascending => "asc",
            SortDirection.Descending => "desc",
            _ => string.Empty
        };
}
