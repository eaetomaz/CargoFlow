using CargoFlow.Application.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Administrador")]
public class AuditLogsController(IAuditLogService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuditLogDto>>> Get([FromQuery] string? entity, [FromQuery] Guid? entityId, CancellationToken cancellationToken) =>
        Ok(await service.GetAsync(entity, entityId, cancellationToken));
}
