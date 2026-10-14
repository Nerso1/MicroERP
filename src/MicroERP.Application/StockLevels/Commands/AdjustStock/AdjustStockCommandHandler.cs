using MediatR;
using MicroERP.Application.Common;
using MicroERP.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MicroERP.Application.StockLevels.Commands.AdjustStock;

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, int>
{
    private readonly IAppDbContext _context;

    public AdjustStockCommandHandler(IAppDbContext context)
        => _context = context;

    public async Task<int> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId
                                   && s.WarehouseId == request.WarehouseId, ct)
            ?? throw new StockLevelNotFoundException(request.ProductId, request.WarehouseId);

        var newQuantity = stockLevel.Quantity + request.QuantityChange;

        if (newQuantity < 0)
            throw new InsufficientStockException(
                request.ProductId,
                request.WarehouseId,
                requested: -request.QuantityChange,
                available: stockLevel.Quantity);

        stockLevel.Quantity = newQuantity;
        await _context.SaveChangesAsync(ct);

        return stockLevel.Quantity;
    }
}
