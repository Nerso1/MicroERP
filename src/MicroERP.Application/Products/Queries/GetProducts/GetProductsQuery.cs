using MediatR;
using MicroERP.Application.Common;

namespace MicroERP.Application.Products.Queries.GetProducts;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null
) : IRequest<PagedResult<ProductDto>>;
