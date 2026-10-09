using FluentAssertions;
using MicroERP.Application.Common;
using MicroERP.Application.Products.Commands.UpdateProduct;
using MicroERP.Domain.Entities;
using MicroERP.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MicroERP.UnitTests.Products.Commands;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IAppDbContext> _contextMock;
    private readonly Mock<DbSet<Product>> _productsMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _productsMock = new Mock<DbSet<Product>>();
        _contextMock = new Mock<IAppDbContext>();
        _contextMock.Setup(c => c.Products).Returns(_productsMock.Object);
        _handler = new UpdateProductCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsProductNotFoundException()
    {
        _productsMock
            .Setup(p => p.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult<Product?>(null));

        var command = new UpdateProductCommand(99, "New Name", null, "SKU-99", 5.00m, true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ProductNotFoundException>()
            .WithMessage("*99*");
    }
}
