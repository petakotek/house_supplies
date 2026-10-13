using HouseholdSupplies.Api.DAL.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdSupplies.Api.DAL.EF.Configurations;

public class ShoppingListItemEntityConfiguration : IEntityTypeConfiguration<ShoppingListItemEntity>
{
    public void Configure(EntityTypeBuilder<ShoppingListItemEntity> modelBuilder)
    {
        modelBuilder
            .HasOne(entry => entry.ShoppingList)
            .WithMany(list => list.Items)
            .HasForeignKey(entry => entry.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .HasOne(entry => entry.Item)
            .WithMany(item => item.ShoppingListItems)
            .HasForeignKey(entry => entry.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}