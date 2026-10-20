using HouseholdSupplies.Api.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdSupplies.Api.DAL.Configurations;

public class ShoppingListEntityConfiguration : IEntityTypeConfiguration<ShoppingListEntity>
{
    public void Configure(EntityTypeBuilder<ShoppingListEntity> modelBuilder)
    {
        modelBuilder
            .HasOne(list => list.Author)
            .WithMany(user => user.ShoppingLists)
            .HasForeignKey(list => list.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}