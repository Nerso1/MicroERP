using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroERP.Application.StockLevels.Commands.AdjustStock;
using MicroERP.Domain.Entities;
using MicroERP.IntegrationTests.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicroERP.IntegrationTests.StockLevels;

[Collection(IntegrationTestCollection.Name)]
public class StockAdjustmentTests : IntegrationTestBase
{
    private const int WarehouseId = 1;

    public StockAdjustmentTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Adjust_DropsBelowThreshold_SavesQuantityAndPublishesAlert()
    {
        var productId = await SeedStockAsync(quantity: 12);

        var response = await Client.PostAsJsonAsync(
            AdjustmentsUrl(productId), new AdjustStockCommand(productId, WarehouseId, -3));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AdjustStockResult>();
        result.Should().Be(new AdjustStockResult(productId, WarehouseId, 9));

        (await GetQuantityAsync(productId)).Should().Be(9);

        var alert = AlertPublisher.Messages.Should().ContainSingle().Subject;
        alert.ProductId.Should().Be(productId);
        alert.WarehouseId.Should().Be(WarehouseId);
        alert.Quantity.Should().Be(9);
    }

    [Fact]
    public async Task Adjust_StaysAboveThreshold_DoesNotPublishAlert()
    {
        var productId = await SeedStockAsync(quantity: 50);

        var response = await Client.PostAsJsonAsync(
            AdjustmentsUrl(productId), new AdjustStockCommand(productId, WarehouseId, -5));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetQuantityAsync(productId)).Should().Be(45);
        AlertPublisher.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Adjust_InsufficientStock_Returns409AndKeepsQuantity()
    {
        var productId = await SeedStockAsync(quantity: 5);

        var response = await Client.PostAsJsonAsync(
            AdjustmentsUrl(productId), new AdjustStockCommand(productId, WarehouseId, -10));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Detail.Should().Contain("Insufficient stock");
        (await GetQuantityAsync(productId)).Should().Be(5);
        AlertPublisher.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Adjust_NoStockInWarehouse_Returns404()
    {
        var productId = await SeedStockAsync(quantity: 20);

        var response = await Client.PostAsJsonAsync(
            AdjustmentsUrl(productId), new AdjustStockCommand(productId, WarehouseId: 2, QuantityChange: -1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Adjust_ZeroChange_Returns400()
    {
        var productId = await SeedStockAsync(quantity: 20);

        var response = await Client.PostAsJsonAsync(
            AdjustmentsUrl(productId), new AdjustStockCommand(productId, WarehouseId, 0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Should().ContainKey("QuantityChange");
    }

    private static string AdjustmentsUrl(int productId) => $"/api/products/{productId}/stock/adjustments";

    private async Task<int> SeedStockAsync(int quantity)
    {
        var product = new Product { Name = "Widget A", Sku = "WGT-001", Price = 9.99m, IsActive = true };

        await ExecuteDbAsync(async db =>
        {
            db.Products.Add(product);
            db.StockLevels.Add(new StockLevel { Product = product, WarehouseId = WarehouseId, Quantity = quantity });
            await db.SaveChangesAsync();
        });

        return product.Id;
    }

    private Task<int> GetQuantityAsync(int productId) =>
        QueryDbAsync(db => db.StockLevels
            .Where(s => s.ProductId == productId && s.WarehouseId == WarehouseId)
            .Select(s => s.Quantity)
            .SingleAsync());
}
