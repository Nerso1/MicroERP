using MediatR;
using Microsoft.AspNetCore.Mvc;
using MicroERP.Application.Products.Commands.CreateProduct;
using MicroERP.Application.Products.Commands.UpdateProduct;
using MicroERP.Application.Products.Commands.DeleteProduct;
using MicroERP.Application.Products.Queries.GetProducts;
using MicroERP.Application.Products.Queries.GetProductById;
using MicroERP.Application.StockLevels.Queries.GetStockByProduct;

namespace MicroERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetProductsQuery(page, pageSize, search));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }

    [HttpGet("{id}/stock")]
    public async Task<IActionResult> GetStock(int id)
    {
        var result = await _mediator.Send(new GetStockByProductQuery(id));
        return Ok(result);
    }
}
