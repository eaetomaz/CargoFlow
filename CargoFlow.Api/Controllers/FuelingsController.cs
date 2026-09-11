using CargoFlow.Application.Fuelings;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api")]
public class FuelingsController(IFuelingService service) : ControllerBase
{
    [HttpGet("fuelings")]
    public async Task<ActionResult<List<FuelingDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("fuelings/{id:guid}")]
    public async Task<ActionResult<FuelingDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var fueling = await service.GetByIdAsync(id, cancellationToken);
        return fueling is null ? NotFound() : Ok(fueling);
    }

    [HttpPost("fuelings")]
    public async Task<ActionResult<FuelingDto>> Create([FromBody] CreateFuelingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var fueling = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = fueling.Id }, fueling);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("vehicles/{vehicleId:guid}/fuel-efficiency")]
    public async Task<ActionResult<FuelEfficiencyDto>> GetFuelEfficiency(Guid vehicleId, CancellationToken cancellationToken) =>
        Ok(await service.GetFuelEfficiencyAsync(vehicleId, cancellationToken));
}
