using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MicroERP.Application.Products.Commands.CreateProduct;
using MicroERP.Application.Products.Commands.UpdateProduct;
using MicroERP.IntegrationTests.Common;

namespace MicroERP.IntegrationTests.ErrorHandling;

[Collection(IntegrationTestCollection.Name)]
public class ProblemDetailsShapeTests : IntegrationTestBase
{
    private const string BaseUrl = "/api/products";

    public ProblemDetailsShapeTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ValidationError_ReturnsCommonFieldsAndErrors()
    {
        var response = await Client.PostAsJsonAsync(BaseUrl, new CreateProductCommand("", null, "WGT-001", 9.99m));

        var problem = await AssertCommonShapeAsync(response, HttpStatusCode.BadRequest);
        problem.TryGetProperty("errors", out _).Should().BeTrue("validation errors should list invalid fields");
    }

    [Fact]
    public async Task RouteBodyIdMismatch_ReturnsCommonFieldsAndDetail()
    {
        var command = new UpdateProductCommand(2, "Widget A", null, "WGT-001", 9.99m, true);

        var response = await Client.PutAsJsonAsync($"{BaseUrl}/1", command);

        var problem = await AssertCommonShapeAsync(response, HttpStatusCode.BadRequest);
        GetString(problem, "detail").Should().Contain("does not match");
    }

    [Fact]
    public async Task NotFound_ReturnsCommonFieldsAndDetail()
    {
        var response = await Client.GetAsync($"{BaseUrl}/999999");

        var problem = await AssertCommonShapeAsync(response, HttpStatusCode.NotFound);
        GetString(problem, "detail").Should().Contain("999999");
    }

    [Fact]
    public async Task Conflict_ReturnsCommonFieldsAndDetail()
    {
        var product = new CreateProductCommand("Widget A", null, "WGT-001", 9.99m);
        (await Client.PostAsJsonAsync(BaseUrl, product)).EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync(BaseUrl, product);

        var problem = await AssertCommonShapeAsync(response, HttpStatusCode.Conflict);
        GetString(problem, "detail").Should().NotBeNullOrEmpty();
    }

    private static async Task<JsonElement> AssertCommonShapeAsync(
        HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        response.StatusCode.Should().Be(expectedStatus);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        GetString(problem, "type").Should().NotBeNullOrEmpty();
        GetString(problem, "title").Should().NotBeNullOrEmpty();
        GetString(problem, "traceId").Should().NotBeNullOrEmpty();
        problem.GetProperty("status").GetInt32().Should().Be((int)expectedStatus);

        return problem;
    }

    private static string? GetString(JsonElement problem, string propertyName)
    {
        problem.TryGetProperty(propertyName, out var value)
            .Should().BeTrue($"ProblemDetails should contain '{propertyName}'");
        return value.GetString();
    }
}
