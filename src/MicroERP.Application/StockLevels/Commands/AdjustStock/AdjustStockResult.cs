namespace MicroERP.Application.StockLevels.Commands.AdjustStock;

public record AdjustStockResult(
    int ProductId,
    int WarehouseId,
    int Quantity
);
