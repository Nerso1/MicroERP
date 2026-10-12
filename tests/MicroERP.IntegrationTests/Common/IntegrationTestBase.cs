using MicroERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MicroERP.IntegrationTests.Common;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
    }

    protected HttpClient Client { get; }

    // xUnit creates a new class instance per test, so this runs before every test.
    public Task InitializeAsync() => ExecuteDbAsync(async db =>
    {
        // Seeded warehouses are kept; products and stock levels start empty.
        await db.StockLevels.ExecuteDeleteAsync();
        await db.Products.ExecuteDeleteAsync();
    });

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task ExecuteDbAsync(Func<AppDbContext, Task> action)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await action(db);
    }

    protected async Task<T> QueryDbAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(db);
    }
}
