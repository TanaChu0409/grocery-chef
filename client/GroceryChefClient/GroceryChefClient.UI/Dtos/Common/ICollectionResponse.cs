namespace GroceryChefClient.UI.Dtos.Common;

public interface ICollectionResponse<T>
{
    List<T> Items { get; init; }
}
