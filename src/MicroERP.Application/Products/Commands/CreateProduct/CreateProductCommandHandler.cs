using MediatR;
using MicroERP.Application.Common;
using MicroERP.Domain.Entities;

namespace MicroERP.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IAppDbContext _context;

    public CreateProductCommandHandler(IAppDbContext context)
        => _context = context;

    public async Task<int> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Sku = request.Sku,
            Price = request.Price,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);

        return product.Id;
    }
}
