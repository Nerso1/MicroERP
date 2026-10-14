using FluentAssertions;
using MicroERP.Application.Common;
using MicroERP.Application.StockLevels.Commands.AdjustStock;
using MicroERP.Application.StockLevels.Events;
using MicroERP.Domain.Entities;
using MicroERP.Domain.Exceptions;
using MicroERP.UnitTests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace MicroERP.UnitTests.StockLevels.Commands;

public class AdjustStockCommandHandlerTests
{
    private const int ProductId = 1;
    private const int WarehouseId = 1;

    private readonly Mock<IAppDbContext> _contextMock;
    private readonly Mock<IStockAlertPublisher> _publisherMock;
    private readonly AdjustStockCommandHandler _handler;

    public AdjustStockCommandHandlerTests()
    {
        _contextMock = new Mock<IAppDbContext>();
        _publisherMock = new Mock<IStockAlertPublisher>();
        _handler = new AdjustStockCommandHandler(
            _contextMock.Object,
            _publisherMock.Object,
            NullLogger<AdjustStockCommandHandler>.Instance);
    }

    [Theory]
    [InlineData(12, -3)]
    [InlineData(10, -1)]
    public async Task Handle_QuantityDropsBelowThreshold_PublishesAlertOnce(int initial, int change)
    {
        SetupStock(initial);

        var result = await _handler.Handle(
            new AdjustStockCommand(ProductId, WarehouseId, change), CancellationToken.None);

        result.Quantity.Should().Be(initial + change);
        _publisherMock.Verify(p => p.PublishAsync(
                It.Is<StockLowAlertMessage>(m =>
                    m.ProductId == ProductId &&
                    m.WarehouseId == WarehouseId &&
                    m.Quantity == initial + change),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(50, -5)]
    [InlineData(11, -1)]
    [InlineData(8, -2)]
    [InlineData(5, 3)]
    public async Task Handle_ThresholdNotCrossed_DoesNotPublishAlert(int initial, int change)
    {
        SetupStock(initial);

        await _handler.Handle(
            new AdjustStockCommand(ProductId, WarehouseId, change), CancellationToken.None);

        _publisherMock.Verify(p => p.PublishAsync(
                It.IsAny<StockLowAlertMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ResultWouldBeNegative_ThrowsAndDoesNotSaveOrPublish()
    {
        SetupStock(5);

        var act = () => _handler.Handle(
            new AdjustStockCommand(ProductId, WarehouseId, -10), CancellationToken.None);

        await act.Should().ThrowAsync<InsufficientStockException>();
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(p => p.PublishAsync(
                It.IsAny<StockLowAlertMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_PublisherFails_StillSavesAndReturnsNewQuantity()
    {
        SetupStock(12);
        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<StockLowAlertMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service Bus unavailable"));

        var result = await _handler.Handle(
            new AdjustStockCommand(ProductId, WarehouseId, -3), CancellationToken.None);

        result.Quantity.Should().Be(9);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StockLevelNotFound_ThrowsStockLevelNotFoundException()
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<StockLevel>());
        _contextMock.Setup(c => c.StockLevels).Returns(mockSet.Object);

        var act = () => _handler.Handle(
            new AdjustStockCommand(ProductId, WarehouseId, -1), CancellationToken.None);

        await act.Should().ThrowAsync<StockLevelNotFoundException>();
    }

    private void SetupStock(int quantity)
    {
        var stockLevel = new StockLevel
        {
            Id = 1,
            ProductId = ProductId,
            WarehouseId = WarehouseId,
            Quantity = quantity
        };

        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<StockLevel> { stockLevel });
        _contextMock.Setup(c => c.StockLevels).Returns(mockSet.Object);
    }
}
