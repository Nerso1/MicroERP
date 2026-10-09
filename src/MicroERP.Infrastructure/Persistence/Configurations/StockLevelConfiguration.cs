using MicroERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroERP.Infrastructure.Persistence.Configurations;

public class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        builder.ToTable("StockLevels", "Products");

        builder.Property(sl => sl.RowVersion)
            .IsRowVersion();

        builder.HasOne(sl => sl.Product)
            .WithMany(p => p.StockLevels)
            .HasForeignKey(sl => sl.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sl => sl.Warehouse)
            .WithMany(w => w.StockLevels)
            .HasForeignKey(sl => sl.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
