using MediatR;
using MicroERP.Application.Common;
using MicroERP.Domain.Exceptions;

namespace MicroERP.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IAppDbContext _context;

    public DeleteProductCommandHandler(IAppDbContext context)
        => _context = context;

    public async Task Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await _context.Products.FindAsync([request.Id], ct)
            ?? throw new ProductNotFoundException(request.Id);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(ct);
    }
}
