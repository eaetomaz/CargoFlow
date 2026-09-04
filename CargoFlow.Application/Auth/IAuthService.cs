namespace CargoFlow.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
}
