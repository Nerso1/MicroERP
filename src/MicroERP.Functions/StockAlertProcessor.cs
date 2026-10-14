using System.Text.Json;
using Azure.Messaging.ServiceBus;
using MicroERP.Functions.Messages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MicroERP.Functions;

public class StockAlertProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ILogger<StockAlertProcessor> _logger;

    public StockAlertProcessor(ILogger<StockAlertProcessor> logger) => _logger = logger;

    [Function(nameof(StockAlertProcessor))]
    public Task Run(
        [ServiceBusTrigger("stock-alerts", Connection = "AzureServiceBus")] ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        var alert = message.Body.ToObjectFromJson<StockLowAlertMessage>(JsonOptions)
            ?? throw new InvalidOperationException($"Message {message.MessageId} has an empty body.");

        _logger.LogWarning(
            "Low stock: product {ProductId} in warehouse {WarehouseId} has {Quantity} units (alert {AlertId}, delivery {DeliveryCount})",
            alert.ProductId, alert.WarehouseId, alert.Quantity, alert.AlertId, message.DeliveryCount);

        return Task.CompletedTask;
    }
}
