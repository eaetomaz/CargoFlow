using CargoFlow.Application.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/accounts-payable")]
[Authorize(Roles = "Administrador,Financeiro")]
public class AccountsPayableController(IAccountPayableService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AccountPayableDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<AccountPayableDto>> Create([FromBody] CreateAccountPayableRequest request, CancellationToken cancellationToken)
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

    [HttpPost("{id:guid}/pay")]
    public async Task<ActionResult<AccountPayableDto>> Pay(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.PayAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<AccountPayableDto>> Cancel(Guid id, CancellationToken cancellationToken)
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
