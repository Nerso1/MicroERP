using MediatR;
using MicroERP.Application.Common;
using MicroERP.Domain.Exceptions;

namespace MicroERP.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IAppDbContext _context;

    public UpdateProductCommandHandler(IAppDbContext context)
        => _context = context;

    public async Task Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _context.Products.FindAsync([request.Id], ct)
            ?? throw new ProductNotFoundException(request.Id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Sku = request.Sku;
        product.Price = request.Price;
        product.IsActive = request.IsActive;

        await _context.SaveChangesAsync(ct);
    }
}
