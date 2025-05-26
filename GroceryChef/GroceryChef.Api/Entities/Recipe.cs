using System;

namespace GroceryChef.Api.Entities;

public sealed class Recipe
{
    private Recipe()
    {
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Content { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static Recipe Create(
        string name,
        string? descriptions,
        string content,
        DateTime createAtUtc)
    {
        return new Recipe
        {
            Id = $"R_{Ulid.NewUlid()}",
            Name = name,
            Content = content,
            Description = descriptions,
            CreatedAtUtc = createAtUtc,
            IsArchived = false
        };
    }
}
