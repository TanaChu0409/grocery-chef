namespace GroceryChef.Api.Entities;

public sealed class Cart
{
    private readonly List<Ingredient> _ingredients = [];
    private Cart()
    {
    }
    public string Id { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients;
    public static Cart Create(string name)
    {
        return new Cart
        {
            Name = name
        };
    }
}
