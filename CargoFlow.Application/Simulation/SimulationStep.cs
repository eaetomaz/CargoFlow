namespace CargoFlow.Application.Simulation;

public record SimulationStepResult(SimulationEventDto? Event, List<SimulationStep> FollowUpSteps);

// Um passo do roteiro de uma viagem simulada. Execute recebe o
// IServiceProvider de um escopo de DI válido (criado pelo SimulationEngine
// a cada tick) e o instante virtual atual -- assim um passo pode agendar
// continuações relativas a "agora" (ex: resolver a ocorrência que ele
// mesmo acabou de criar) sem precisar saber o Id de antemão.
public record SimulationStep(DateTime VirtualTime, Guid TripId, string Description, SimulationStepExecutor Execute);

public delegate Task<SimulationStepResult> SimulationStepExecutor(IServiceProvider services, DateTime virtualNow, CancellationToken cancellationToken);
