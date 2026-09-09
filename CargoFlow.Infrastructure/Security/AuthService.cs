using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CargoFlow.Application.Auth;
using CargoFlow.Domain.Entities.Auth;
using CargoFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CargoFlow.Infrastructure.Security;

public class AuthService(CargoFlowDbContext context, IConfiguration configuration) : IAuthService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(12);
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, cancellationToken);

        if (user is null)
            return null;

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return null;

        var expiresAt = DateTime.UtcNow.Add(TokenLifetime);
        var token = GenerateToken(user, expiresAt);

        return new LoginResponse(token, expiresAt, ToDto(user));
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return user is null ? null : ToDto(user);
    }

    private string GenerateToken(User user, DateTime expiresAt)
    {
        var secret = configuration["JWT_SECRET"]
            ?? throw new InvalidOperationException("JWT_SECRET não configurado.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto ToDto(User user) =>
        new(user.Id, user.Username, user.Email, user.FullName, user.Role.ToString(), user.DriverId);
}
