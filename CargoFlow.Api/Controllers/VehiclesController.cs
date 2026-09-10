using CargoFlow.Application.Fleet;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController(IVehicleService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<VehicleDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await service.GetByIdAsync(id, cancellationToken);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> Create([FromBody] UpsertVehicleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var vehicle = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VehicleDto>> Update(Guid id, [FromBody] UpsertVehicleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.UpdateAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
