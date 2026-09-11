using CargoFlow.Application.Freight;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/freight-quotes")]
public class FreightQuotesController(IFreightQuoteService service) : ControllerBase
{
    [HttpPost("calculate")]
    public async Task<ActionResult<FreightQuoteBreakdown>> Calculate([FromBody] CalculateFreightQuoteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.CalculateAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<FreightQuoteDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FreightQuoteDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var quote = await service.GetByIdAsync(id, cancellationToken);
        return quote is null ? NotFound() : Ok(quote);
    }

    [HttpPost]
    public async Task<ActionResult<FreightQuoteDto>> Create([FromBody] CreateFreightQuoteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var quote = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = quote.Id }, quote);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<FreightQuoteDto>> Approve(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ApproveAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<FreightQuoteDto>> Reject(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.RejectAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
