using FluentAssertions;
using MicroERP.Application.Common;
using MicroERP.Application.Products.Commands.CreateProduct;
using MicroERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MicroERP.UnitTests.Products.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IAppDbContext> _contextMock;
    private readonly Mock<DbSet<Product>> _productsMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productsMock = new Mock<DbSet<Product>>();
        _contextMock = new Mock<IAppDbContext>();
        _contextMock.Setup(c => c.Products).Returns(_productsMock.Object);
        _handler = new CreateProductCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsProductAndSavesOnce()
    {
        _contextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateProductCommand("Widget A", "A widget", "WGT-001", 9.99m);

        await _handler.Handle(command, CancellationToken.None);

        _productsMock.Verify(
            p => p.Add(It.Is<Product>(x =>
                x.Name == "Widget A" &&
                x.Sku == "WGT-001" &&
                x.Price == 9.99m &&
                x.IsActive)),
            Times.Once);

        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSku_ThrowsDbUpdateException()
    {
        // Handler nie sprawdza duplikatów — wyjątek pochodzi z bazy (unikalny indeks na SKU).
        // Test symuluje to zachowanie przez wyrzucenie DbUpdateException z SaveChangesAsync.
        _contextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Unique constraint violation"));

        var command = new CreateProductCommand("Widget B", null, "WGT-001", 19.99m);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
