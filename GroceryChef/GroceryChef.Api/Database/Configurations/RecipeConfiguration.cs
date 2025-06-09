using GroceryChef.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryChef.Api.Database.Configurations;

internal sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasMaxLength(500);

        builder.Property(r => r.Name).HasMaxLength(500);

        builder.Property(r => r.Description).HasMaxLength(100);

        builder.Property(r => r.Content).HasMaxLength(3000);

        builder.HasMany(r => r.Ingredients)
            .WithMany()
            .UsingEntity<RecipeIngredient>();
    }
}
