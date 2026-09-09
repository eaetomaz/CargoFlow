using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
using CargoFlow.Application.Finance;
using CargoFlow.Application.Audit;
using CargoFlow.Application.Dashboard;
using CargoFlow.Application.Simulation;

namespace CargoFlow.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentComplianceService, DocumentComplianceService>();
        services.AddScoped<IFreightPricingRuleService, FreightPricingRuleService>();
        services.AddSingleton<IFreightQuoteCalculationService, FreightQuoteCalculationService>();
        services.AddScoped<IFreightQuoteService, FreightQuoteService>();
        services.AddScoped<ITransportOrderService, TransportOrderService>();
        services.AddSingleton<ITripSchedulingRules, TripSchedulingRules>();
        services.AddSingleton<ITripFinancialService, TripFinancialService>();
        services.AddScoped<ITripSchedulingService, TripSchedulingService>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<IOccurrenceService, OccurrenceService>();
        services.AddSingleton<IFuelingOdometerValidator, FuelingOdometerValidator>();
        services.AddScoped<IFuelingService, FuelingService>();
        services.AddScoped<IMaintenanceOrderService, MaintenanceOrderService>();
        services.AddScoped<IAccountPayableService, AccountPayableService>();
        services.AddScoped<IAccountReceivableService, AccountReceivableService>();
        services.AddScoped<IFinanceSummaryService, FinanceSummaryService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // Singleton de propósito -- guarda o estado (relógio virtual, fila de
        // passos, assinantes SSE) entre requisições HTTP independentes.
        services.AddSingleton<ISimulationEngine, SimulationEngine>();

        return services;
    }
}
