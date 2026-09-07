namespace CargoFlow.Application.Trips;

public interface ITripService
{
    Task<List<TripDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<TripDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TripDto> StartAsync(Guid id, CancellationToken cancellationToken);
    Task<TripDto> AddExpenseAsync(Guid id, AddTripExpenseRequest request, CancellationToken cancellationToken);
    Task<TripDto> CompleteDeliveryAsync(Guid id, CompleteTripDeliveryRequest request, CancellationToken cancellationToken);
    Task<TripDto> CancelAsync(Guid id, CancellationToken cancellationToken);
    Task<TripProfitabilityDto> GetProfitabilityAsync(Guid id, CancellationToken cancellationToken);
}
