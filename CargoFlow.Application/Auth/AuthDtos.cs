namespace CargoFlow.Application.Auth;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt, UserDto User);

public record UserDto(Guid Id, string Username, string Email, string FullName, string Role, Guid? DriverId);
