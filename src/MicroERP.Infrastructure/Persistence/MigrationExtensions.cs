using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MicroERP.Infrastructure.Persistence;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        // DbContext is registered as scoped, so it cannot be resolved from the root provider.
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
