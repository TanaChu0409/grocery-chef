namespace GroceryChef.Api.Entities;

public sealed class Cart
{
    private Cart()
    {
    }
    public string Id { get; private set; }
    public string Name { get; private set; }
    public static Cart Create(string name)
    {
        return new Cart
        {
            Name = name
        };
    }
}
