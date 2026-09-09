using CargoFlow.Domain.Common;
using CargoFlow.Domain.Entities.Audit;
using CargoFlow.Domain.Entities.Auth;
using CargoFlow.Domain.Entities.Companies;
using CargoFlow.Domain.Entities.Documents;
using CargoFlow.Domain.Entities.Drivers;
using CargoFlow.Domain.Entities.Finance;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.Freight;
using CargoFlow.Domain.Entities.Fuelings;
using CargoFlow.Domain.Entities.Maintenance;
using CargoFlow.Domain.Entities.Occurrences;
using CargoFlow.Domain.Entities.TransportOrders;
using CargoFlow.Domain.Entities.Trips;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence;

public class CargoFlowDbContext(DbContextOptions<CargoFlowDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Contact> Contacts => Set<Contact>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Driver> Drivers => Set<Driver>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<User> Users => Set<User>();

    public DbSet<FreightPricingRule> FreightPricingRules => Set<FreightPricingRule>();
    public DbSet<FreightQuote> FreightQuotes => Set<FreightQuote>();

    public DbSet<TransportOrder> TransportOrders => Set<TransportOrder>();

    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripExpense> TripExpenses => Set<TripExpense>();
    public DbSet<TripEvent> TripEvents => Set<TripEvent>();

    public DbSet<Occurrence> Occurrences => Set<Occurrence>();
    public DbSet<OccurrenceAttachment> OccurrenceAttachments => Set<OccurrenceAttachment>();

    public DbSet<Fueling> Fuelings => Set<Fueling>();

    public DbSet<MaintenanceOrder> MaintenanceOrders => Set<MaintenanceOrder>();

    public DbSet<AccountPayable> AccountsPayable => Set<AccountPayable>();
    public DbSet<AccountReceivable> AccountsReceivable => Set<AccountReceivable>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CargoFlowDbContext).Assembly);

        // Toda entidade (via Entity.Id) gera a chave no construtor, client-side
        // -- sem isso, o EF Core às vezes rastreia uma entidade nova como
        // "Modified" em vez de "Added" quando ela chega ao change tracker só
        // por fixup de navegação (ex: company.Addresses.Add(novoEndereco) numa
        // Company já rastreada), virando um UPDATE de uma linha que não existe
        // (DbUpdateConcurrencyException). Aplicado uma vez pra todas as
        // entidades em vez de repetir em cada IEntityTypeConfiguration.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
                modelBuilder.Entity(entityType.ClrType).Property(nameof(Entity.Id)).ValueGeneratedNever();
        }

        base.OnModelCreating(modelBuilder);
    }
}
