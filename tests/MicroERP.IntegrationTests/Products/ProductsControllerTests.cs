using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroERP.Application.Common;
using MicroERP.Application.Products.Commands.CreateProduct;
using MicroERP.Application.Products.Queries.GetProducts;
using MicroERP.Domain.Entities;
using MicroERP.IntegrationTests.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicroERP.IntegrationTests.Products;

[Collection(IntegrationTestCollection.Name)]
public class ProductsControllerTests : IntegrationTestBase
{
    private const string BaseUrl = "/api/products";

    public ProductsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_ValidProduct_Returns201AndSavesProduct()
    {
        var command = new CreateProductCommand("Widget A", "Test product", "WGT-001", 9.99m);

        var response = await Client.PostAsJsonAsync(BaseUrl, command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<int>();
        response.Headers.Location!.ToString().Should().EndWithEquivalentOf($"{BaseUrl}/{id}");

        var product = await QueryDbAsync(db => db.Products.SingleOrDefaultAsync(p => p.Id == id));
        product.Should().NotBeNull();
        product!.Sku.Should().Be("WGT-001");
        product.Price.Should().Be(9.99m);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Create_EmptyName_Returns400AndDoesNotSaveProduct()
    {
        var command = new CreateProductCommand("", null, "WGT-001", 9.99m);

        var response = await Client.PostAsJsonAsync(BaseUrl, command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Should().ContainKey("Name");
        (await QueryDbAsync(db => db.Products.CountAsync())).Should().Be(0);
    }

    [Fact]
    public async Task Create_NegativePrice_Returns400AndDoesNotSaveProduct()
    {
        var command = new CreateProductCommand("Widget A", null, "WGT-001", -5m);

        var response = await Client.PostAsJsonAsync(BaseUrl, command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Should().ContainKey("Price");
        (await QueryDbAsync(db => db.Products.CountAsync())).Should().Be(0);
    }

    [Fact]
    public async Task Create_DuplicateSku_Returns409AndKeepsSingleProduct()
    {
        var first = new CreateProductCommand("Widget A", null, "WGT-001", 9.99m);
        var duplicate = new CreateProductCommand("Widget B", null, "WGT-001", 19.99m);
        (await Client.PostAsJsonAsync(BaseUrl, first)).EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync(BaseUrl, duplicate);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.Should().Be(StatusCodes.Status409Conflict);
        (await QueryDbAsync(db => db.Products.CountAsync(p => p.Sku == "WGT-001"))).Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingProduct_Returns404WithProblemDetails()
    {
        var response = await Client.GetAsync($"{BaseUrl}/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Detail.Should().Contain("999999");
    }

    [Fact]
    public async Task GetAll_SecondPage_ReturnsRemainingItemsAndTotalCount()
    {
        await ExecuteDbAsync(async db =>
        {
            for (var i = 1; i <= 15; i++)
            {
                db.Products.Add(new Product
                {
                    Name = $"Product {i:D2}",
                    Sku = $"SKU-{i:D3}",
                    Price = 10m,
                    IsActive = true
                });
            }

            await db.SaveChangesAsync();
        });

        var response = await Client.GetAsync($"{BaseUrl}?page=2&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        result!.TotalCount.Should().Be(15);
        result.Page.Should().Be(2);
        result.Items.Should().HaveCount(5);
        result.Items.First().Name.Should().Be("Product 11");
    }
}
