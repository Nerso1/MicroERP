using FluentAssertions;
using MicroERP.Application.Common;
using MicroERP.Application.Products.Queries.GetProducts;
using MicroERP.Domain.Entities;
using MicroERP.UnitTests.Helpers;
using Moq;

namespace MicroERP.UnitTests.Products.Queries;

public class GetProductsQueryHandlerTests
{
    private readonly Mock<IAppDbContext> _contextMock;
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTests()
    {
        _contextMock = new Mock<IAppDbContext>();
        _handler = new GetProductsQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPage()
    {
        var products = Enumerable.Range(1, 15)
            .Select(i => new Product
            {
                Id = i,
                Name = $"Product {i:D2}",
                Sku = $"SKU-{i:D3}",
                Price = i * 1.0m,
                IsActive = true
            })
            .ToList();

        var mockSet = MockDbSetHelper.CreateMockDbSet(products);
        _contextMock.Setup(c => c.Products).Returns(mockSet.Object);

        var query = new GetProductsQuery(Page: 2, PageSize: 5);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(15);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
    }
}
