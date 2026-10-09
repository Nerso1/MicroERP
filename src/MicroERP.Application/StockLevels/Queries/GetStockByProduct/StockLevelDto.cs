namespace MicroERP.Application.StockLevels.Queries.GetStockByProduct;

public record StockLevelDto(
    int WarehouseId,
    string WarehouseName,
    int Quantity
);
