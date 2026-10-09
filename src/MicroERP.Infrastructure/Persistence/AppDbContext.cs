using MicroERP.Domain.Entities;
using MicroERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MicroERP.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;

            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Warehouse>().HasData(
            new Warehouse { Id = 1, Name = "Main Warehouse",      Location = "Cracow",  IsActive = true, CreatedAt = seededAt },
            new Warehouse { Id = 2, Name = "Regional Warehouse",  Location = "Warsaw",    IsActive = true, CreatedAt = seededAt },
            new Warehouse { Id = 3, Name = "External Warehouse",  Location = "Prague",    IsActive = true, CreatedAt = seededAt }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "PLA Filament 1.75mm - Jet Black (1kg)",    Sku = "FIL-PLA-BLK-1KG", Price = 19.99m, IsActive = true,  CreatedAt = seededAt },
            new Product { Id = 2, Name = "PLA Filament 1.75mm - Signal White (1kg)", Sku = "FIL-PLA-WHT-1KG", Price = 19.99m, IsActive = true,  CreatedAt = seededAt },
            new Product { Id = 3, Name = "PETG Filament 1.75mm - Jet Black (1kg)",   Sku = "FIL-PETG-BLK1K", Price = 22.49m, IsActive = true,  CreatedAt = seededAt },
            new Product { Id = 4, Name = "Hardened Steel Nozzle 0.4mm",              Sku = "PAR-NOZ-HS04",   Price = 9.99m,  IsActive = true,  CreatedAt = seededAt },
            new Product { Id = 5, Name = "Textured PEI Build Plate 257x257mm",       Sku = "ACC-PEI-257",    Price = 29.99m, IsActive = true,  CreatedAt = seededAt },
            new Product { Id = 6, Name = "3D Print Finishing Tool Set",              Sku = "TOO-POST-SET",   Price = 14.50m, IsActive = true,  CreatedAt = seededAt },
            new Product { Id = 7, Name = "Filament Dry Box Container",               Sku = "ACC-DRY-BOX1",   Price = 42.99m, IsActive = false, CreatedAt = seededAt }
        );

        modelBuilder.Entity<StockLevel>().HasData(
            new StockLevel { Id = 1,  ProductId = 1, WarehouseId = 1, Quantity = 150, CreatedAt = seededAt },
            new StockLevel { Id = 2,  ProductId = 1, WarehouseId = 2, Quantity = 60,  CreatedAt = seededAt },
            new StockLevel { Id = 3,  ProductId = 2, WarehouseId = 1, Quantity = 45,  CreatedAt = seededAt },
            new StockLevel { Id = 4,  ProductId = 2, WarehouseId = 3, Quantity = 20,  CreatedAt = seededAt },
            new StockLevel { Id = 5,  ProductId = 3, WarehouseId = 1, Quantity = 30,  CreatedAt = seededAt },
            new StockLevel { Id = 6,  ProductId = 4, WarehouseId = 1, Quantity = 12,  CreatedAt = seededAt },
            new StockLevel { Id = 7,  ProductId = 4, WarehouseId = 2, Quantity = 8,   CreatedAt = seededAt },
            new StockLevel { Id = 8,  ProductId = 5, WarehouseId = 1, Quantity = 75,  CreatedAt = seededAt },
            new StockLevel { Id = 9,  ProductId = 6, WarehouseId = 2, Quantity = 200, CreatedAt = seededAt },
            new StockLevel { Id = 10, ProductId = 7, WarehouseId = 3, Quantity = 5,   CreatedAt = seededAt }
        );
    }
}
