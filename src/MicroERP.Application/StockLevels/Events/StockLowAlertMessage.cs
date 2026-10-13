namespace MicroERP.Application.StockLevels.Events;

public record StockLowAlertMessage(
    Guid AlertId,
    int ProductId,
    int WarehouseId,
    int Quantity,
    DateTime OccurredAtUtc
);
