using HouseholdSupplies.Api.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdSupplies.Api.DAL.Configurations;

public class PlaceEntityConfiguration : IEntityTypeConfiguration<PlaceEntity>
{

    public void Configure(EntityTypeBuilder<PlaceEntity> modelBuilder)
    {
        modelBuilder
            .HasOne(place => place.Author)
            .WithMany(user => user.Places)
            .HasForeignKey(place => place.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    
}