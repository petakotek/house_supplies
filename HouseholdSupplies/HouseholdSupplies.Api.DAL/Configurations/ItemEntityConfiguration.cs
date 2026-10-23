using HouseholdSupplies.Api.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdSupplies.Api.DAL.Configurations;

public class ItemEntityConfiguration : IEntityTypeConfiguration<ItemEntity>
{
    public void Configure(EntityTypeBuilder<ItemEntity> modelBuilder)
    {
        modelBuilder
            .HasOne(item => item.Author)
            .WithMany(user => user.Items)
            .HasForeignKey(item => item.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder
            .HasOne(item => item.Place)
            .WithMany(place => place.Items)
            .HasForeignKey(item => item.PlaceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}