using CargoFlow.Application.Companies;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController(ICompanyService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CompanyDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var company = await service.GetByIdAsync(id, cancellationToken);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create([FromBody] UpsertCompanyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var company = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> Update(Guid id, [FromBody] UpsertCompanyRequest request, CancellationToken cancellationToken)
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
