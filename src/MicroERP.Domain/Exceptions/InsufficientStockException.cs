namespace MicroERP.Domain.Exceptions;

public class InsufficientStockException : Exception
{
    public InsufficientStockException(int productId, int warehouseId, int requested, int available)
        : base($"Insufficient stock for product {productId} in warehouse {warehouseId}. Requested: {requested}, available: {available}.")
    {
    }
}
