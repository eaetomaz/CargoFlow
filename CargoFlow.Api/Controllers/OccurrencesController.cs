using CargoFlow.Application.Occurrences;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/occurrences")]
public class OccurrencesController(IOccurrenceService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<OccurrenceDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OccurrenceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var occurrence = await service.GetByIdAsync(id, cancellationToken);
        return occurrence is null ? NotFound() : Ok(occurrence);
    }

    [HttpPost]
    public async Task<ActionResult<OccurrenceDto>> Create([FromBody] CreateOccurrenceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var occurrence = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = occurrence.Id }, occurrence);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<ActionResult<OccurrenceDto>> Resolve(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ResolveAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OccurrenceDto>> Cancel(Guid id, CancellationToken cancellationToken)
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

    [HttpPost("{id:guid}/attachments")]
    public async Task<ActionResult<OccurrenceDto>> AddAttachment(Guid id, [FromBody] AddOccurrenceAttachmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.AddAttachmentAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
