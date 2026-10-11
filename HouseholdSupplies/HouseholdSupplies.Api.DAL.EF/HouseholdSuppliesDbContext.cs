using HouseholdSupplies.Api.DAL.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace HouseholdSupplies.Api.DAL.EF;

public class HouseholdSuppliesDbContext : DbContext
{
    public DbSet<ItemEntity> Items { get; set; } = null!;
    public DbSet<PlaceEntity> Places { get; set; } = null!;
    public DbSet<ShoppingListEntity> ShoppingLists { get; set; } = null!;
    public DbSet<ShoppingListItemEntity> ShoppingListItems { get; set; } = null!;
    public DbSet<UserEntity> Users { get; set; } = null!;

    public HouseholdSuppliesDbContext(DbContextOptions<HouseholdSuppliesDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<ItemEntity>()
            .HasOne(item => item.Author)
            .WithMany(user => user.Items)
            .HasForeignKey(item => item.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PlaceEntity>()
            .HasOne(place => place.Author)
            .WithMany(user => user.Places)
            .HasForeignKey(place => place.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<ShoppingListEntity>()
            .HasOne(list => list.Author)
            .WithMany(user => user.ShoppingLists)
            .HasForeignKey(list => list.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<ItemEntity>()
            .HasOne(item => item.Place)
            .WithMany(place => place.Items)
            .HasForeignKey(item => item.PlaceId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<ShoppingListItemEntity>()
            .HasOne(entry => entry.ShoppingList)
            .WithMany(list => list.Items)
            .HasForeignKey(entry => entry.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShoppingListItemEntity>()
            .HasOne(entry => entry.Item)
            .WithMany(item => item.ShoppingListItems)
            .HasForeignKey(entry => entry.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}