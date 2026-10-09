using MediatR;
using MicroERP.Application.Products.Queries.GetProducts;

namespace MicroERP.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;
