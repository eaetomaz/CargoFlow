using CargoFlow.Application.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/accounts-receivable")]
[Authorize(Roles = "Administrador,Financeiro")]
public class AccountsReceivableController(IAccountReceivableService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AccountReceivableDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<AccountReceivableDto>> Create([FromBody] CreateAccountReceivableRequest request, CancellationToken cancellationToken)
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

    [HttpPost("{id:guid}/receive")]
    public async Task<ActionResult<AccountReceivableDto>> Receive(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ReceiveAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<AccountReceivableDto>> Cancel(Guid id, CancellationToken cancellationToken)
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
