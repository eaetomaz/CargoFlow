using CargoFlow.Application.Maintenance;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/maintenance-orders")]
public class MaintenanceOrdersController(IMaintenanceOrderService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MaintenanceOrderDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MaintenanceOrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await service.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<MaintenanceOrderDto>> Create([FromBody] CreateMaintenanceOrderRequest request, CancellationToken cancellationToken)
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

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<MaintenanceOrderDto>> Start(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.StartAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<MaintenanceOrderDto>> Complete(Guid id, [FromBody] CompleteMaintenanceOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.CompleteAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<MaintenanceOrderDto>> Cancel(Guid id, CancellationToken cancellationToken)
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
