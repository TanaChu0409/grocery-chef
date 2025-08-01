﻿using GroceryChef.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryChef.Api.Database.Configurations;

internal sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasMaxLength(500);
        builder.Property(c => c.UserId).HasMaxLength(500);

        builder.Property(c => c.Name).HasMaxLength(500);

        builder.HasMany(c => c.Ingredients)
            .WithMany()
            .UsingEntity<CartIngredient>();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(h => h.UserId);
    }
}
