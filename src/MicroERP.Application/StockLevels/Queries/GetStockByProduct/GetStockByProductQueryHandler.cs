using MediatR;
using MicroERP.Application.Common;
using MicroERP.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MicroERP.Application.StockLevels.Queries.GetStockByProduct;

public class GetStockByProductQueryHandler : IRequestHandler<GetStockByProductQuery, List<StockLevelDto>>
{
    private readonly IAppDbContext _context;

    public GetStockByProductQueryHandler(IAppDbContext context)
        => _context = context;

    public async Task<List<StockLevelDto>> Handle(GetStockByProductQuery request, CancellationToken ct)
    {
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == request.ProductId, ct);

        if (!productExists)
            throw new ProductNotFoundException(request.ProductId);

        return await _context.StockLevels
            .Where(s => s.ProductId == request.ProductId)
            .Select(s => new StockLevelDto(s.WarehouseId, s.Warehouse.Name, s.Quantity))
            .ToListAsync(ct);
    }
}
