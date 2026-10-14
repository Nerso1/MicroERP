using System.Text.Json;
using Azure.Messaging.ServiceBus;
using MicroERP.Functions.Messages;
using MicroERP.Functions.Processing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MicroERP.Functions;

public class StockAlertProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IProcessedMessageStore _processedMessages;
    private readonly ILogger<StockAlertProcessor> _logger;

    public StockAlertProcessor(IProcessedMessageStore processedMessages, ILogger<StockAlertProcessor> logger)
    {
        _processedMessages = processedMessages;
        _logger = logger;
    }

    [Function(nameof(StockAlertProcessor))]
    public async Task Run(
        [ServiceBusTrigger("stock-alerts", Connection = "AzureServiceBus")] ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        if (!await _processedMessages.TryMarkProcessedAsync(message.MessageId, cancellationToken))
        {
            _logger.LogInformation(
                "Skipping duplicate message {MessageId} (delivery {DeliveryCount})",
                message.MessageId, message.DeliveryCount);
            return;
        }

        try
        {
            var alert = message.Body.ToObjectFromJson<StockLowAlertMessage>(JsonOptions)
                ?? throw new InvalidOperationException($"Message {message.MessageId} has an empty body.");

            _logger.LogWarning(
                "Low stock: product {ProductId} in warehouse {WarehouseId} has {Quantity} units (alert {AlertId}, delivery {DeliveryCount})",
                alert.ProductId, alert.WarehouseId, alert.Quantity, alert.AlertId, message.DeliveryCount);
        }
        catch
        {
            // Release the claim so the redelivery is processed instead of skipped as a duplicate.
            await _processedMessages.UnmarkAsync(message.MessageId, CancellationToken.None);
            throw;
        }
    }
}
