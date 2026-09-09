using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CargoFlow.Infrastructure.Persistence;

// Usado só por "dotnet ef migrations add/update" via linha de comando, fora
// do host da Api. Lê a connection string de variável de ambiente.
public class CargoFlowDbContextFactory : IDesignTimeDbContextFactory<CargoFlowDbContext>
{
    public CargoFlowDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION")
            ?? "Data Source=cargoflow.db";

        var optionsBuilder = new DbContextOptionsBuilder<CargoFlowDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new CargoFlowDbContext(optionsBuilder.Options);
    }
}
