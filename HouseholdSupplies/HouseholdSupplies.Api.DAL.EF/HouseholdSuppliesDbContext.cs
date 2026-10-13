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
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HouseholdSuppliesDbContext).Assembly);
    }
}