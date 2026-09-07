namespace CargoFlow.Application.Trips;

public interface ITripSchedulingService
{
    Task<TripDto> ScheduleTripAsync(ScheduleTripRequest request, CancellationToken cancellationToken);
}
