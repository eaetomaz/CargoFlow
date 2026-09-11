using CargoFlow.Application.TransportOrders;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/transport-orders")]
public class TransportOrdersController(ITransportOrderService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TransportOrderDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransportOrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await service.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<TransportOrderDto>> Create([FromBody] CreateTransportOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public record GenerateFromQuoteRequest(string CargoDescription, string RequestedPickupDate, string RequestedDeliveryDate);

    [HttpPost("from-quote/{freightQuoteId:guid}")]
    public async Task<ActionResult<TransportOrderDto>> CreateFromQuote(Guid freightQuoteId, [FromBody] GenerateFromQuoteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await service.CreateFromFreightQuoteAsync(freightQuoteId, request.CargoDescription, request.RequestedPickupDate, request.RequestedDeliveryDate, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<TransportOrderDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.CancelAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
