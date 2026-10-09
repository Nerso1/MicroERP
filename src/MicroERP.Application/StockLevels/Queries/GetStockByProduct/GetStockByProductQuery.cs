using MediatR;

namespace MicroERP.Application.StockLevels.Queries.GetStockByProduct;

public record GetStockByProductQuery(int ProductId) : IRequest<List<StockLevelDto>>;
