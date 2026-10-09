using MediatR;

namespace MicroERP.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string? Description,
    string Sku,
    decimal Price
) : IRequest<int>;
