using MicroERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroERP.Application.Common;

public interface IAppDbContext
{
    DbSet<Product> Products { get; }
    DbSet<StockLevel> StockLevels { get; }
    DbSet<Warehouse> Warehouses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
