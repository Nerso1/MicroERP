using MediatR;

namespace MicroERP.Application.StockLevels.Commands.AdjustStock;

public record AdjustStockCommand(
    int ProductId,
    int WarehouseId,
    int QuantityChange
) : IRequest<AdjustStockResult>;
