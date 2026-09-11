using CargoFlow.Application.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken cancellationToken) =>
        Ok(await service.GetDashboardAsync(cancellationToken));
}
