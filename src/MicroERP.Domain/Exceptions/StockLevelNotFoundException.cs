namespace MicroERP.Domain.Exceptions;

public class StockLevelNotFoundException : Exception
{
    public StockLevelNotFoundException(int productId, int warehouseId)
        : base($"No stock record for product {productId} in warehouse {warehouseId}.")
    {
    }
}
