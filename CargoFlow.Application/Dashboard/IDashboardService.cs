namespace CargoFlow.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken);
}
