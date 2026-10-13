using MicroERP.Application.StockLevels.Events;

namespace MicroERP.Application.Common;

public interface IStockAlertPublisher
{
    Task PublishAsync(StockLowAlertMessage message, CancellationToken cancellationToken = default);
}
