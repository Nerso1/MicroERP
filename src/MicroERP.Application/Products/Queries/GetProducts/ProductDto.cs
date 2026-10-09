namespace MicroERP.Application.Products.Queries.GetProducts;

public record ProductDto(
    int Id,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    bool IsActive
);
