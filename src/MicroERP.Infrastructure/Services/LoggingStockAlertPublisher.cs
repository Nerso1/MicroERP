using MicroERP.Application.Common;
using MicroERP.Application.StockLevels.Events;
using Microsoft.Extensions.Logging;

namespace MicroERP.Infrastructure.Services;

public class LoggingStockAlertPublisher : IStockAlertPublisher
{
    private readonly ILogger<LoggingStockAlertPublisher> _logger;

    public LoggingStockAlertPublisher(ILogger<LoggingStockAlertPublisher> logger)
        => _logger = logger;

    public Task PublishAsync(StockLowAlertMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Azure Service Bus is not configured. Low stock alert {AlertId}: product {ProductId}, warehouse {WarehouseId}, quantity {Quantity}",
            message.AlertId, message.ProductId, message.WarehouseId, message.Quantity);

        return Task.CompletedTask;
    }
}
