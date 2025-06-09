using GroceryChef.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryChef.Api.Database.Configurations;

internal sealed class CartIngredientConfiguration : IEntityTypeConfiguration<CartIngredient>
{
    public void Configure(EntityTypeBuilder<CartIngredient> builder)
    {
        builder.Property(ci => ci.CartId).HasMaxLength(500);
        builder.Property(ci => ci.IngrdientId).HasMaxLength(500);
    }
}
