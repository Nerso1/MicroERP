using FluentAssertions;
using MicroERP.Application.Products.Commands.CreateProduct;

namespace MicroERP.UnitTests.Validators;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public async Task Validate_EmptyName_ReturnsValidationError()
    {
        var command = new CreateProductCommand(string.Empty, null, "SKU-001", 9.99m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public async Task Validate_NegativePrice_ReturnsValidationError()
    {
        var command = new CreateProductCommand("Widget", null, "SKU-001", -1.00m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateProductCommand.Price));
    }
}
