using CargoFlow.Application.Drivers;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/drivers")]
public class DriversController(IDriverService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DriverDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DriverDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var driver = await service.GetByIdAsync(id, cancellationToken);
        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpPost]
    public async Task<ActionResult<DriverDto>> Create([FromBody] UpsertDriverRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var driver = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = driver.Id }, driver);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DriverDto>> Update(Guid id, [FromBody] UpsertDriverRequest request, CancellationToken cancellationToken)
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
