using CargoFlow.Application.Documents;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController(IDocumentService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DocumentDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("expiring")]
    public async Task<ActionResult<List<DocumentDto>>> GetExpiring(CancellationToken cancellationToken) =>
        Ok(await service.GetExpiringAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var document = await service.GetByIdAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(document);
    }

    [HttpPost]
    public async Task<ActionResult<DocumentDto>> Create([FromBody] UpsertDocumentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var document = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> Update(Guid id, [FromBody] UpsertDocumentRequest request, CancellationToken cancellationToken)
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
