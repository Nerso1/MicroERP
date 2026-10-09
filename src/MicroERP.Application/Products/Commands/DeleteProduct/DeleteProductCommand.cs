using MediatR;

namespace MicroERP.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int Id) : IRequest;

