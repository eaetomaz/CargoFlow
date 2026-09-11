using CargoFlow.Application.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/finance")]
[Authorize(Roles = "Administrador,Financeiro")]
public class FinanceController(IFinanceSummaryService service) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<FinanceSummaryDto>> GetSummary(CancellationToken cancellationToken) =>
        Ok(await service.GetSummaryAsync(cancellationToken));
}
