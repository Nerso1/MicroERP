using System.Collections.Concurrent;
using MicroERP.Application.Common;
using MicroERP.Application.StockLevels.Events;

namespace MicroERP.IntegrationTests.Common;

public class FakeStockAlertPublisher : IStockAlertPublisher
{
    private readonly ConcurrentQueue<StockLowAlertMessage> _messages = new();

    public IReadOnlyCollection<StockLowAlertMessage> Messages => _messages;

    public Task PublishAsync(StockLowAlertMessage message, CancellationToken cancellationToken = default)
    {
        _messages.Enqueue(message);
        return Task.CompletedTask;
    }

    public void Clear() => _messages.Clear();
}
