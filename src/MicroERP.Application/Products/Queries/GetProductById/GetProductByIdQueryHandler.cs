using MediatR;
using MicroERP.Application.Common;
using MicroERP.Application.Products.Queries.GetProducts;
using MicroERP.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MicroERP.Application.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IAppDbContext _context;

    public GetProductByIdQueryHandler(IAppDbContext context)
        => _context = context;

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await _context.Products
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Sku, p.Price, p.IsActive))
            .FirstOrDefaultAsync(ct)
            ?? throw new ProductNotFoundException(request.Id);

        return product;
    }
}
