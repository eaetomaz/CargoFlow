namespace CargoFlow.Application.Simulation;

public enum SimulationRunStatus
{
    Idle,
    Running,
    Finished,
}

public record SimulationStatusDto(
    string Status,
    string? StartedAtVirtual,
    string? CurrentVirtualTime,
    int SpeedMultiplier,
    int ScheduledTripCount,
    int CompletedTripCount,
    int PendingStepCount);

// Type é o vocabulário da seção 9 do documento (TripStarted, FuelingRegistered
// etc.) como string -- serve tanto pra log legível quanto pra o front decidir
// ícone/cor sem precisar mapear um enum C# extra no TypeScript.
public record SimulationEventDto(string Type, string OccurredAtVirtual, string Message, Guid? TripId);

public record StartSimulationRequest(int? SpeedMultiplier);
