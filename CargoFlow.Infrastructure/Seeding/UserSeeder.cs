using CargoFlow.Domain.Entities.Auth;
using CargoFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Seeding;

// Roda no startup, só se a tabela Users estiver vazia -- garante login
// possível logo no primeiro "dotnet run" num clone novo, um por perfil de
// RBAC. Senha de dev igual pros 5 (documentada no README), nunca usada fora
// de ambiente local.
public static class UserSeeder
{
    public const string DevPassword = "CargoFlow@123";

    public static async Task SeedAsync(CargoFlowDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(cancellationToken))
            return;

        var hasher = new PasswordHasher<User>();

        var users = new[]
        {
            new User { Username = "admin", Email = "admin@cargoflow.local", FullName = "Administrador", Role = UserRole.Administrador },
            new User { Username = "operacional", Email = "operacional@cargoflow.local", FullName = "Operador", Role = UserRole.Operacional },
            new User { Username = "financeiro", Email = "financeiro@cargoflow.local", FullName = "Financeiro", Role = UserRole.Financeiro },
            new User { Username = "manutencao", Email = "manutencao@cargoflow.local", FullName = "Manutenção", Role = UserRole.Manutencao },
            new User { Username = "motorista", Email = "motorista@cargoflow.local", FullName = "Motorista Exemplo", Role = UserRole.Motorista },
        };

        foreach (var user in users)
            user.PasswordHash = hasher.HashPassword(user, DevPassword);

        context.Users.AddRange(users);
        await context.SaveChangesAsync(cancellationToken);
    }
}
