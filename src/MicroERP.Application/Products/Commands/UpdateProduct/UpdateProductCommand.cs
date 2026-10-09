using MediatR;

namespace MicroERP.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    int Id,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    bool IsActive
) : IRequest;

