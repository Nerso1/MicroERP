using FluentValidation;

namespace MicroERP.Application.StockLevels.Commands.AdjustStock;

public class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0);

        RuleFor(x => x.QuantityChange)
            .NotEqual(0);
    }
}
