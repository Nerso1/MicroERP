using Azure.Messaging.ServiceBus;
using MicroERP.Application.Common;
using MicroERP.Application.StockLevels.Events;

namespace MicroERP.Infrastructure.Services;

public class ServiceBusStockAlertPublisher : IStockAlertPublisher
{
    private readonly ServiceBusSender _sender;

    public ServiceBusStockAlertPublisher(ServiceBusClient client, string queueName)
        => _sender = client.CreateSender(queueName);

    public async Task PublishAsync(StockLowAlertMessage message, CancellationToken cancellationToken = default)
    {
        var serviceBusMessage = new ServiceBusMessage(BinaryData.FromObjectAsJson(message))
        {
            MessageId = message.AlertId.ToString(),
            ContentType = "application/json",
            Subject = nameof(StockLowAlertMessage)
        };

        await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }
}
