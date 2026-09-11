using CargoFlow.Application.Freight;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/freight-pricing-rules")]
public class FreightPricingRulesController(IFreightPricingRuleService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<FreightPricingRuleDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<FreightPricingRuleDto>> Create([FromBody] UpsertFreightPricingRuleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.CreateAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FreightPricingRuleDto>> Update(Guid id, [FromBody] UpsertFreightPricingRuleRequest request, CancellationToken cancellationToken)
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
