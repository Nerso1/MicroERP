namespace MicroERP.Functions.Messages;

public record StockLowAlertMessage(
    Guid AlertId,
    int ProductId,
    int WarehouseId,
    int Quantity,
    DateTime OccurredAtUtc
);
