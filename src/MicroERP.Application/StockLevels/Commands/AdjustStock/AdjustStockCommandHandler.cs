using MediatR;
using MicroERP.Application.Common;
using MicroERP.Application.StockLevels.Events;
using MicroERP.Domain.Entities;
using MicroERP.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MicroERP.Application.StockLevels.Commands.AdjustStock;

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, int>
{
    public const int LowStockThreshold = 10;

    private readonly IAppDbContext _context;
    private readonly IStockAlertPublisher _alertPublisher;
    private readonly ILogger<AdjustStockCommandHandler> _logger;

    public AdjustStockCommandHandler(
        IAppDbContext context,
        IStockAlertPublisher alertPublisher,
        ILogger<AdjustStockCommandHandler> logger)
    {
        _context = context;
        _alertPublisher = alertPublisher;
        _logger = logger;
    }

    public async Task<int> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId
                                   && s.WarehouseId == request.WarehouseId, ct)
            ?? throw new StockLevelNotFoundException(request.ProductId, request.WarehouseId);

        var previousQuantity = stockLevel.Quantity;
        var newQuantity = previousQuantity + request.QuantityChange;

        if (newQuantity < 0)
            throw new InsufficientStockException(
                request.ProductId,
                request.WarehouseId,
                requested: -request.QuantityChange,
                available: previousQuantity);

        stockLevel.Quantity = newQuantity;
        await _context.SaveChangesAsync(ct);

        if (previousQuantity >= LowStockThreshold && newQuantity < LowStockThreshold)
            await PublishLowStockAlertAsync(stockLevel);

        return stockLevel.Quantity;
    }

    private async Task PublishLowStockAlertAsync(StockLevel stockLevel)
    {
        var message = new StockLowAlertMessage(
            Guid.NewGuid(),
            stockLevel.ProductId,
            stockLevel.WarehouseId,
            stockLevel.Quantity,
            DateTime.UtcNow);

        try
        {
            // The stock change is already saved, so the alert is sent even if the client disconnects
            // (no request CancellationToken), and a failed alert must not become an error response -
            // the client would retry and adjust the stock twice.
            await _alertPublisher.PublishAsync(message, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish low stock alert {AlertId} for product {ProductId} in warehouse {WarehouseId}",
                message.AlertId, message.ProductId, message.WarehouseId);
        }
    }
}
