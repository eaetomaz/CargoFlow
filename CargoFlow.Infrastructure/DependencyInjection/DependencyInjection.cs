using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CargoFlow.Application.Auth;
using CargoFlow.Application.Companies;
using CargoFlow.Application.Documents;
using CargoFlow.Application.Drivers;
using CargoFlow.Application.Fleet;
using CargoFlow.Application.Freight;
using CargoFlow.Application.Fuelings;
using CargoFlow.Application.Maintenance;
using CargoFlow.Application.Occurrences;
using CargoFlow.Application.TransportOrders;
using CargoFlow.Application.Trips;
using CargoFlow.Infrastructure.Persistence;
using CargoFlow.Infrastructure.Persistence.Repositories.Companies;
using CargoFlow.Infrastructure.Persistence.Repositories.Documents;
using CargoFlow.Infrastructure.Persistence.Repositories.Drivers;
using CargoFlow.Infrastructure.Persistence.Repositories.Fleet;
using CargoFlow.Infrastructure.Persistence.Repositories.Freight;
using CargoFlow.Infrastructure.Persistence.Repositories.Fuelings;
using CargoFlow.Infrastructure.Persistence.Repositories.Maintenance;
using CargoFlow.Infrastructure.Persistence.Repositories.Occurrences;
using CargoFlow.Infrastructure.Persistence.Repositories.TransportOrders;
using CargoFlow.Infrastructure.Persistence.Repositories.Trips;
using CargoFlow.Infrastructure.Persistence.Repositories.Finance;
using CargoFlow.Infrastructure.Persistence.Repositories.Audit;
using CargoFlow.Infrastructure.Persistence.Interceptors;
using CargoFlow.Application.Finance;
using CargoFlow.Application.Audit;
using CargoFlow.Infrastructure.Security;
using Microsoft.AspNetCore.Http;

namespace CargoFlow.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<AuditSaveChangesInterceptor>();

        services.AddDbContext<CargoFlowDbContext>((sp, options) =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IFreightPricingRuleRepository, FreightPricingRuleRepository>();
        services.AddScoped<IFreightQuoteRepository, FreightQuoteRepository>();
        services.AddScoped<ITransportOrderRepository, TransportOrderRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IOccurrenceRepository, OccurrenceRepository>();
        services.AddScoped<IFuelingRepository, FuelingRepository>();
        services.AddScoped<IMaintenanceOrderRepository, MaintenanceOrderRepository>();
        services.AddScoped<IAccountPayableRepository, AccountPayableRepository>();
        services.AddScoped<IAccountReceivableRepository, AccountReceivableRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }
}
