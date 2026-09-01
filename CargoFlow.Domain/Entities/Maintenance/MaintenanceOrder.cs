using CargoFlow.Domain.Common;
using CargoFlow.Domain.Entities.Fleet;

namespace CargoFlow.Domain.Entities.Maintenance;

public enum MaintenanceType
{
    Preventiva,
    Corretiva,
}

public enum MaintenanceOrderStatus
{
    Aberta,
    EmExecucao,
    Concluida,
    Cancelada,
}

public class MaintenanceOrder : Entity, IAuditable
{
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public MaintenanceType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public MaintenanceOrderStatus Status { get; set; } = MaintenanceOrderStatus.Aberta;
    public DateTime OpenedAt { get; set; }
    public DateOnly? ScheduledDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? OdometerAtService { get; set; }
    public decimal? Cost { get; set; }
    public string? ServiceProvider { get; set; }

    public void Start(DateTime now)
    {
        if (Status != MaintenanceOrderStatus.Aberta)
            throw new InvalidOperationException($"Só é possível iniciar uma ordem \"Aberta\" -- esta está \"{Status}\".");

        Status = MaintenanceOrderStatus.EmExecucao;
        StartedAt = now;
        UpdateTimestamp();
    }

    public void Complete(DateTime now, decimal cost, decimal? odometerAtService)
    {
        if (Status != MaintenanceOrderStatus.EmExecucao)
            throw new InvalidOperationException($"Só é possível concluir uma ordem \"EmExecucao\" -- esta está \"{Status}\".");

        Status = MaintenanceOrderStatus.Concluida;
        CompletedAt = now;
        Cost = cost;
        OdometerAtService = odometerAtService;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == MaintenanceOrderStatus.Concluida)
            throw new InvalidOperationException("Ordem concluída não pode ser cancelada.");
        if (Status == MaintenanceOrderStatus.Cancelada)
            return; // idempotente

        Status = MaintenanceOrderStatus.Cancelada;
        UpdateTimestamp();
    }
}
