using Bogus;
using CargoFlow.Application.Freight;
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
using CargoFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Seeding;

// ATENÇÃO: dados 100% fictícios, gerados com Bogus (locale pt_BR), só pra
// popular o portfólio/projeto de estudo com algo navegável. Nenhum nome,
// CPF/CNPJ, placa ou CNH aqui corresponde a pessoa/empresa/veículo real.
// Roda só se Companies estiver vazia (banco novo) -- Users já foi semeado
// pelo UserSeeder antes disso, então não dá pra usar Users como gate.
public static class DemoDataSeederService
{
    private static readonly string[] TruckBrands = ["Volvo", "Scania", "Mercedes-Benz", "Iveco", "DAF", "MAN", "Ford", "Volkswagen"];
    private static readonly string[] TruckModels = ["FH 540", "R 450", "Actros 2651", "Stralis 570", "XF 105", "TGX 29.480", "Cargo 2429", "Constellation 24.280"];

    public static async Task SeedAsync(CargoFlowDbContext context, int expiringSoonThresholdDays, CancellationToken cancellationToken)
    {
        if (await context.Companies.AnyAsync(cancellationToken))
            return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var companies = SeedCompanies();
        context.Companies.AddRange(companies);
        await context.SaveChangesAsync(cancellationToken);

        var vehicles = SeedVehicles();
        var drivers = SeedDrivers(today);
        context.Vehicles.AddRange(vehicles);
        context.Drivers.AddRange(drivers);
        await context.SaveChangesAsync(cancellationToken);

        var documents = SeedDocuments(companies, vehicles, drivers, today, expiringSoonThresholdDays);
        context.Documents.AddRange(documents);
        await context.SaveChangesAsync(cancellationToken);

        var pricingRules = SeedFreightPricingRules(today);
        context.FreightPricingRules.AddRange(pricingRules);
        await context.SaveChangesAsync(cancellationToken);

        var quotes = SeedFreightQuotes(companies, pricingRules);
        context.FreightQuotes.AddRange(quotes);
        await context.SaveChangesAsync(cancellationToken);

        var orders = SeedTransportOrders(companies);
        context.TransportOrders.AddRange(orders);
        await context.SaveChangesAsync(cancellationToken);

        var trips = SeedTrips(orders, vehicles, drivers, today);
        context.Trips.AddRange(trips);
        await context.SaveChangesAsync(cancellationToken);

        var occurrences = SeedOccurrences(trips, vehicles, drivers, today);
        context.Occurrences.AddRange(occurrences);
        await context.SaveChangesAsync(cancellationToken);

        var fuelings = SeedFuelings(trips, vehicles, drivers, today);
        context.Fuelings.AddRange(fuelings);
        await context.SaveChangesAsync(cancellationToken);

        var maintenanceOrders = SeedMaintenanceOrders(vehicles, trips, today);
        context.MaintenanceOrders.AddRange(maintenanceOrders);
        await context.SaveChangesAsync(cancellationToken);

        var payables = SeedAccountsPayable(companies, maintenanceOrders, today);
        context.AccountsPayable.AddRange(payables);
        await context.SaveChangesAsync(cancellationToken);

        // As viagens já "Concluida" no seed foram criadas direto no domínio
        // (SeedTrips), não passaram por TripService/BillingGenerationHandler
        // -- replica aqui o mesmo efeito (regra 6) pra já ter contas a
        // receber reais pra mostrar, coerentes com as viagens entregues.
        var receivables = SeedAccountsReceivable(orders, trips, today);
        context.AccountsReceivable.AddRange(receivables);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<AccountPayable> SeedAccountsPayable(List<Company> companies, List<MaintenanceOrder> maintenanceOrders, DateOnly today)
    {
        var faker = new Faker("pt_BR");
        var suppliers = companies.Where(c => c.Roles.HasFlag(CompanyRoles.Fornecedor)).ToList();
        var payables = new List<AccountPayable>();

        foreach (var order in maintenanceOrders.Where(o => o.Status == MaintenanceOrderStatus.Concluida))
        {
            payables.Add(new AccountPayable
            {
                Category = AccountPayableCategory.Manutencao,
                Description = $"Manutenção {order.Type} -- OS {order.Id.ToString()[..8]}",
                SupplierCompanyId = suppliers.Count > 0 ? faker.PickRandom(suppliers).Id : null,
                SourceType = nameof(MaintenanceOrder),
                SourceId = order.Id,
                Amount = order.Cost ?? 0,
                DueDate = order.CompletedAt is { } completed ? DateOnly.FromDateTime(completed) : today,
                Status = AccountPayableStatus.Pago,
                PaidDate = order.CompletedAt is { } paid ? DateOnly.FromDateTime(paid) : today,
            });
        }

        // Mais algumas despesas avulsas (pedágio/fornecedor/despesa), com uma
        // mistura proposital de pendente/vencida/paga.
        var categories = new[] { AccountPayableCategory.Pedagio, AccountPayableCategory.Fornecedor, AccountPayableCategory.Despesa };
        for (var i = 0; i < 6; i++)
        {
            var dueDate = today.AddDays(faker.Random.Int(-15, 20));
            var status = dueDate < today ? AccountPayableStatus.Vencido : faker.Random.Bool(0.4f) ? AccountPayableStatus.Pago : AccountPayableStatus.Pendente;

            payables.Add(new AccountPayable
            {
                Category = faker.Random.ArrayElement(categories),
                Description = faker.Commerce.ProductName(),
                SupplierCompanyId = suppliers.Count > 0 && faker.Random.Bool(0.6f) ? faker.PickRandom(suppliers).Id : null,
                Amount = Math.Round(faker.Random.Decimal(150, 4000), 2),
                DueDate = dueDate,
                Status = status,
                PaidDate = status == AccountPayableStatus.Pago ? dueDate.AddDays(-faker.Random.Int(0, 3)) : null,
            });
        }

        return payables;
    }

    private static List<AccountReceivable> SeedAccountsReceivable(List<TransportOrder> orders, List<Trip> trips, DateOnly today)
    {
        var faker = new Faker("pt_BR");
        var receivables = new List<AccountReceivable>();

        foreach (var trip in trips.Where(t => t.Status == TripStatus.Concluida))
        {
            var order = orders.First(o => o.Id == trip.TransportOrderId);
            order.MarkBilled();

            var dueDate = trip.ActualArrivalAt is { } arrival ? DateOnly.FromDateTime(arrival).AddDays(30) : today.AddDays(30);
            var isPastDue = dueDate < today;

            receivables.Add(new AccountReceivable
            {
                CustomerCompanyId = order.CustomerCompanyId,
                TripId = trip.Id,
                TransportOrderId = order.Id,
                Amount = trip.FreightRevenue,
                DueDate = dueDate,
                Status = isPastDue ? AccountReceivableStatus.Vencido : AccountReceivableStatus.Pendente,
            });
        }

        // Um recebível já quitado, avulso, pra ter dado de "recebido" no
        // resumo financeiro desde o primeiro carregamento.
        if (receivables.Count > 0)
        {
            var extra = receivables[0];
            receivables.Add(new AccountReceivable
            {
                CustomerCompanyId = extra.CustomerCompanyId,
                Amount = Math.Round(faker.Random.Decimal(1000, 6000), 2),
                DueDate = today.AddDays(-10),
                ReceivedDate = today.AddDays(-8),
                Status = AccountReceivableStatus.Recebido,
            });
        }

        return receivables;
    }

    // ~6 ocorrências: espalhadas nas viagens já concluídas/em andamento (pra
    // aparecer na timeline delas) e duas avulsas. Uma Quebra dispara o
    // workflow automático (replica aqui o que OccurrenceService.CreateAsync
    // faz de verdade: marca o veículo como EmManutencao).
    private static List<Occurrence> SeedOccurrences(List<Trip> trips, List<Vehicle> vehicles, List<Driver> drivers, DateOnly today)
    {
        var faker = new Faker("pt_BR");
        var linkedTrips = trips.Where(t => t.Status is TripStatus.Concluida or TripStatus.EmAndamento).ToList();
        var occurrences = new List<Occurrence>();

        Occurrence BuildLinked(Trip trip, OccurrenceType type, string location, string description) => new()
        {
            TripId = trip.Id,
            VehicleId = trip.VehicleId,
            DriverId = trip.DriverId,
            Type = type,
            OccurredAt = trip.ActualDepartureAt?.AddHours(faker.Random.Int(1, 20)) ?? DateTime.UtcNow.AddHours(-faker.Random.Int(1, 10)),
            Location = location,
            Description = description,
        };

        if (linkedTrips.Count > 0)
        {
            var o1 = BuildLinked(linkedTrips[0], OccurrenceType.Atraso, $"{faker.Address.City()}/{faker.Address.StateAbbr()}", "Atraso na liberação de carga no cliente.");
            o1.Resolve(o1.OccurredAt.AddHours(3));
            occurrences.Add(o1);
        }

        if (linkedTrips.Count > 1)
            occurrences.Add(BuildLinked(linkedTrips[1], OccurrenceType.CargaAvariada, $"{faker.Address.City()}/{faker.Address.StateAbbr()}", "Avaria em parte da carga durante o transporte."));

        var breakdownTrip = linkedTrips.FirstOrDefault(t => t.Status == TripStatus.EmAndamento) ?? linkedTrips.LastOrDefault();
        if (breakdownTrip is not null)
        {
            var breakdown = BuildLinked(breakdownTrip, OccurrenceType.Quebra, $"{faker.Address.City()}/{faker.Address.StateAbbr()}", "Pane mecânica na estrada, veículo precisou ser rebocado.");
            occurrences.Add(breakdown);

            var vehicle = vehicles.First(v => v.Id == breakdownTrip.VehicleId);
            vehicle.Status = VehicleStatus.EmManutencao;

            occurrences.Add(BuildLinked(breakdownTrip, OccurrenceType.DocumentacaoIrregular, $"{faker.Address.City()}/{faker.Address.StateAbbr()}", "Nota fiscal com divergência de peso."));
        }

        var freeVehicle = faker.PickRandom(vehicles);
        var freeDriver = faker.PickRandom(drivers);
        var o5 = new Occurrence
        {
            VehicleId = freeVehicle.Id,
            DriverId = freeDriver.Id,
            Type = OccurrenceType.PneuFurado,
            OccurredAt = DateTime.UtcNow.AddDays(-faker.Random.Int(2, 15)),
            Location = $"{faker.Address.City()}/{faker.Address.StateAbbr()}",
            Description = "Pneu furado durante manobra no pátio.",
        };
        o5.Resolve(o5.OccurredAt.AddHours(2));
        occurrences.Add(o5);

        occurrences.Add(new Occurrence
        {
            VehicleId = faker.PickRandom(vehicles).Id,
            DriverId = faker.PickRandom(drivers).Id,
            Type = OccurrenceType.ClienteAusente,
            OccurredAt = DateTime.UtcNow.AddDays(-faker.Random.Int(1, 5)),
            Location = $"{faker.Address.City()}/{faker.Address.StateAbbr()}",
            Description = "Cliente ausente no endereço de entrega no horário agendado.",
        });

        return occurrences;
    }

    // ~15 abastecimentos, 2-3 por veículo escolhido, sempre com leituras de
    // odômetro crescentes (respeita a mesma regra 8 que FuelingOdometerValidator
    // aplica em tempo real) e sem ultrapassar o odômetro atual do veículo.
    // Alguns ficam vinculados às viagens já iniciadas (abastecimento durante
    // o trajeto).
    private static List<Fueling> SeedFuelings(List<Trip> trips, List<Vehicle> vehicles, List<Driver> drivers, DateOnly today)
    {
        var faker = new Faker("pt_BR");
        var gasStations = new[] { "Posto Ipiranga BR-101", "Posto Shell Rodovia", "Auto Posto São Cristóvão", "Posto Petrobras Km 45", "Rede Posto Estrela" };
        var startedTrips = trips.Where(t => t.ActualDepartureAt is not null).ToList();

        var candidateVehicles = vehicles.Where(v => v.Odometer > 1000).OrderBy(_ => faker.Random.Int()).Take(6).ToList();
        var fuelings = new List<Fueling>();

        foreach (var vehicle in candidateVehicles)
        {
            var count = faker.Random.Int(2, 3);
            var trip = startedTrips.FirstOrDefault(t => t.VehicleId == vehicle.Id);

            // Trabalha de trás pra frente a partir do odômetro atual, com
            // intervalos realistas de tanque cheio (~400-900km pra um veículo
            // pesado) e consumo plausível (~2-3.5 km/l) -- gera litros a
            // partir da distância, não um valor solto, senão o km/l
            // resultante fica absurdo (achado testando o dashboard ao vivo).
            var readingsDescending = new List<decimal>();
            var reading = vehicle.Odometer;
            for (var i = 0; i < count; i++)
            {
                readingsDescending.Add(reading);
                reading -= faker.Random.Decimal(400, 900);
            }
            readingsDescending.Reverse(); // agora em ordem cronológica (mais antigo primeiro)

            for (var i = 0; i < count; i++)
            {
                var currentReading = Math.Round(Math.Max(readingsDescending[i], 0), 1);
                var gapKm = i == 0 ? faker.Random.Decimal(400, 900) : currentReading - Math.Round(readingsDescending[i - 1], 1);
                var consumptionKmPerLiter = faker.Random.Decimal(2.0m, 3.5m);
                var liters = Math.Max(gapKm / consumptionKmPerLiter, 50m);
                var pricePerLiter = faker.Random.Decimal(5.6m, 6.4m);
                var daysAgo = (count - i) * faker.Random.Int(4, 12);

                var isLastAndLinkedToTrip = trip is not null && i == count - 1;

                fuelings.Add(new Fueling
                {
                    VehicleId = vehicle.Id,
                    DriverId = isLastAndLinkedToTrip ? trip!.DriverId : faker.PickRandom(drivers).Id,
                    TripId = isLastAndLinkedToTrip ? trip!.Id : null,
                    GasStationName = faker.Random.ArrayElement(gasStations),
                    FuelingDate = today.AddDays(-daysAgo),
                    LiterQuantity = Math.Round(liters, 2),
                    TotalValue = Math.Round(liters * pricePerLiter, 2),
                    OdometerReading = currentReading,
                });
            }
        }

        return fuelings;
    }

    // ~5 ordens de manutenção: 2-3 concluídas (históricas, com custo e
    // odômetro registrados) e 1-2 abertas/em execução -- essas últimas
    // usando um veículo fora do conjunto já escalado nas viagens semeadas,
    // pra manter o status EmManutencao coerente com o restante do seed.
    private static List<MaintenanceOrder> SeedMaintenanceOrders(List<Vehicle> vehicles, List<Trip> trips, DateOnly today)
    {
        var faker = new Faker("pt_BR");
        var providers = new[] { "Oficina Central Diesel", "Concessionária Volvo SP", "Truck Center Manutenções", "Auto Elétrica Rodoviária" };
        var tripVehicleIds = trips.Select(t => t.VehicleId).ToHashSet();
        var orders = new List<MaintenanceOrder>();

        var historicalVehicles = faker.PickRandom(vehicles, 3).ToList();
        foreach (var vehicle in historicalVehicles)
        {
            var openedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(30, 180));
            var order = new MaintenanceOrder
            {
                VehicleId = vehicle.Id,
                Type = faker.Random.Enum<MaintenanceType>(),
                Description = faker.Random.ArrayElement(["Troca de óleo e filtros", "Revisão de freios", "Alinhamento e balanceamento", "Substituição de pneus dianteiros"]),
                OpenedAt = openedAt,
                ServiceProvider = faker.Random.ArrayElement(providers),
            };

            order.Start(openedAt.AddHours(faker.Random.Int(2, 24)));
            order.Complete(order.StartedAt!.Value.AddDays(faker.Random.Int(1, 4)), Math.Round(faker.Random.Decimal(200, 3500), 2), Math.Round(vehicle.Odometer * faker.Random.Decimal(0.4m, 0.9m), 1));
            orders.Add(order);
        }

        var openCandidates = vehicles
            .Where(v => !tripVehicleIds.Contains(v.Id) && v.Status == VehicleStatus.Disponivel && historicalVehicles.All(hv => hv.Id != v.Id))
            .ToList();

        var openCount = Math.Min(2, openCandidates.Count);
        foreach (var vehicle in openCount == 0 ? [] : faker.PickRandom(openCandidates, openCount))
        {
            var openedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(0, 5));
            var order = new MaintenanceOrder
            {
                VehicleId = vehicle.Id,
                Type = faker.Random.Enum<MaintenanceType>(),
                Description = faker.Random.ArrayElement(["Barulho estranho na suspensão", "Revisão preventiva programada", "Troca de embreagem"]),
                OpenedAt = openedAt,
                ScheduledDate = today.AddDays(faker.Random.Int(0, 3)),
                ServiceProvider = faker.Random.ArrayElement(providers),
            };

            vehicle.Status = VehicleStatus.EmManutencao;

            if (faker.Random.Bool(0.5f))
                order.Start(openedAt.AddHours(faker.Random.Int(1, 12)));

            orders.Add(order);
        }

        return orders;
    }

    // 10 OTs prontas -- 4 ficam "Criada" (matéria-prima pra programação
    // manual e pro simulador), as outras 6 recebem viagem via SeedTrips.
    private static List<TransportOrder> SeedTransportOrders(List<Company> companies)
    {
        var faker = new Faker("pt_BR");
        var customers = companies.Where(c => c.Roles.HasFlag(CompanyRoles.Cliente)).ToList();
        var orders = new List<TransportOrder>();

        for (var i = 0; i < 10; i++)
        {
            var pickup = DateTime.UtcNow.AddDays(faker.Random.Int(-10, 5));
            var cargoWeight = faker.Random.Decimal(500, 15000);

            orders.Add(new TransportOrder
            {
                CustomerCompanyId = faker.PickRandom(customers).Id,
                OriginCity = faker.Address.City(),
                OriginState = faker.Address.StateAbbr(),
                DestinationCity = faker.Address.City(),
                DestinationState = faker.Address.StateAbbr(),
                CargoDescription = faker.Commerce.ProductName(),
                CargoWeightKg = Math.Round(cargoWeight, 2),
                CargoValue = Math.Round(faker.Random.Decimal(3000, 150000), 2),
                FreightValue = Math.Round(faker.Random.Decimal(800, 12000), 2),
                RequestedPickupDate = pickup,
                RequestedDeliveryDate = pickup.AddDays(faker.Random.Int(1, 5)),
            });
        }

        return orders;
    }

    // Espalha as 6 primeiras OTs em viagens em estados diferentes -- 3
    // concluídas (com despesas e rentabilidade real pra mostrar), 2
    // programadas e 1 em andamento, todas "matéria-prima" coerente pro
    // dashboard e pro simulador não começar do zero vazio.
    private static List<Trip> SeedTrips(List<TransportOrder> orders, List<Vehicle> vehicles, List<Driver> drivers, DateOnly today)
    {
        var faker = new Faker("pt_BR");
        var availableVehicles = new Queue<Vehicle>(vehicles.Where(v => v.Status == VehicleStatus.Disponivel).OrderBy(_ => faker.Random.Int()));
        var availableDrivers = new Queue<Driver>(drivers.Where(d => d.AvailabilityStatus == DriverAvailabilityStatus.Disponivel && d.CnhExpiryDate > today).OrderBy(_ => faker.Random.Int()));

        var trips = new List<Trip>();
        var expenseTypes = Enum.GetValues<TripExpenseType>();

        for (var i = 0; i < 6 && availableVehicles.Count > 0 && availableDrivers.Count > 0; i++)
        {
            var order = orders[i];
            var vehicle = availableVehicles.Dequeue();
            var driver = availableDrivers.Dequeue();
            var distance = faker.Random.Decimal(80, 1500);

            var trip = Trip.Schedule(
                order.Id, vehicle.Id, null, driver.Id,
                order.OriginCity, order.OriginState, order.DestinationCity, order.DestinationState,
                order.RequestedPickupDate, order.RequestedPickupDate.AddDays(2), Math.Round(distance, 1), order.FreightValue);
            order.MarkScheduled();
            vehicle.Status = VehicleStatus.EmViagem;
            driver.AvailabilityStatus = DriverAvailabilityStatus.EmViagem;

            // i=0,1,2 -> concluída; i=3 -> em andamento; i=4,5 -> só programada.
            if (i <= 2)
            {
                trip.Start(order.RequestedPickupDate);
                order.MarkInTransit();

                foreach (var _ in Enumerable.Range(0, faker.Random.Int(1, 3)))
                {
                    trip.AddExpense(new TripExpense
                    {
                        Type = faker.Random.ArrayElement(expenseTypes),
                        Description = faker.Lorem.Sentence(3),
                        Value = Math.Round(faker.Random.Decimal(50, 800), 2),
                        ExpenseDate = DateOnly.FromDateTime(order.RequestedPickupDate),
                    });
                }

                var actualDistance = distance * faker.Random.Decimal(0.95m, 1.1m);
                trip.CompleteDelivery(order.RequestedPickupDate.AddDays(2), Math.Round(actualDistance, 1));
                order.MarkDelivered();
                vehicle.Status = VehicleStatus.Disponivel;
                driver.AvailabilityStatus = DriverAvailabilityStatus.Disponivel;
            }
            else if (i == 3)
            {
                trip.Start(DateTime.UtcNow.AddHours(-6));
                order.MarkInTransit();
                trip.AddExpense(new TripExpense
                {
                    Type = TripExpenseType.Pedagio,
                    Description = "Pedágio BR-101",
                    Value = Math.Round(faker.Random.Decimal(40, 150), 2),
                    ExpenseDate = today,
                });
            }

            trips.Add(trip);
        }

        return trips;
    }

    // Uma regra genérica (CargoType null) por tipo de veículo -- suficiente
    // pra cotação funcionar de imediato num clone novo, sem exigir que o
    // usuário configure preço manualmente antes de conseguir cotar algo.
    private static List<FreightPricingRule> SeedFreightPricingRules(DateOnly today)
    {
        var faker = new Faker("pt_BR");
        var rules = new List<FreightPricingRule>();

        foreach (var vehicleType in Enum.GetValues<VehicleType>())
        {
            // Veículos maiores cobram mais por km/kg -- ordem aproximada de
            // capacidade dos enums (Cavalo/Toco menores, Bitrem/Rodotrem maiores).
            var sizeFactor = vehicleType switch
            {
                VehicleType.Cavalo => 1.0m,
                VehicleType.CaminhaoToco => 0.9m,
                VehicleType.CaminhaoTruck => 1.2m,
                VehicleType.Carreta => 1.4m,
                VehicleType.Bitrem => 1.7m,
                VehicleType.Rodotrem => 2.0m,
                _ => 1.0m,
            };

            rules.Add(new FreightPricingRule
            {
                VehicleType = vehicleType,
                CargoType = null,
                PricePerKg = Math.Round(0.35m * sizeFactor, 4),
                PricePerKm = Math.Round(1.8m * sizeFactor, 4),
                TollPerKm = Math.Round(0.12m * sizeFactor, 4),
                AdValoremPercentage = 0.3m,
                GrisPercentage = 0.1m,
                MinimumFreightValue = Math.Round(150m * sizeFactor, 2),
                EffectiveFrom = today.AddYears(-1),
                EffectiveTo = null,
            });
        }

        return rules;
    }

    private static List<FreightQuote> SeedFreightQuotes(List<Company> companies, List<FreightPricingRule> pricingRules)
    {
        var faker = new Faker("pt_BR");
        var calculationService = new FreightQuoteCalculationService();
        var customers = companies.Where(c => c.Roles.HasFlag(CompanyRoles.Cliente)).ToList();
        var quotes = new List<FreightQuote>();

        var statuses = new[]
        {
            FreightQuoteStatus.Sent, FreightQuoteStatus.Sent, FreightQuoteStatus.Sent,
            FreightQuoteStatus.Approved, FreightQuoteStatus.Approved,
            FreightQuoteStatus.Rejected,
        };

        for (var i = 0; i < 12; i++)
        {
            var customer = faker.PickRandom(customers);
            var cargoType = faker.Random.Enum<CargoType>();
            var vehicleType = faker.Random.Enum<VehicleType>();
            var rule = pricingRules.First(r => r.VehicleType == vehicleType && r.CargoType == null);

            var weightKg = faker.Random.Decimal(500, 20000);
            var cargoValue = faker.Random.Decimal(2000, 200000);
            var distanceKm = faker.Random.Decimal(80, 2200);
            var otherCosts = faker.Random.Bool(0.3f) ? faker.Random.Decimal(20, 300) : 0m;

            var breakdown = calculationService.Calculate(rule, weightKg, cargoValue, distanceKm, otherCosts);

            quotes.Add(new FreightQuote
            {
                CustomerCompanyId = customer.Id,
                OriginCity = faker.Address.City(),
                OriginState = faker.Address.StateAbbr(),
                DestinationCity = faker.Address.City(),
                DestinationState = faker.Address.StateAbbr(),
                CargoType = cargoType,
                CargoWeightKg = Math.Round(weightKg, 2),
                CargoValue = Math.Round(cargoValue, 2),
                RequiredVehicleType = vehicleType,
                EstimatedDistanceKm = Math.Round(distanceKm, 1),
                FreightWeightValue = breakdown.FreightWeightValue,
                TollValue = breakdown.TollValue,
                AdValoremValue = breakdown.AdValoremValue,
                GrisValue = breakdown.GrisValue,
                OtherCostsValue = breakdown.OtherCostsValue,
                TotalValue = breakdown.TotalValue,
                Status = faker.Random.ArrayElement(statuses),
                ExpiresAt = DateTime.UtcNow.AddDays(faker.Random.Int(5, 30)),
            });
        }

        return quotes;
    }

    private static List<Company> SeedCompanies()
    {
        var faker = new Faker("pt_BR");
        var usedDocuments = new HashSet<string>();
        var roleOptions = new[] { CompanyRoles.Cliente, CompanyRoles.Embarcador, CompanyRoles.Destinatario, CompanyRoles.Fornecedor, CompanyRoles.Parceiro };
        var companies = new List<Company>();

        for (var i = 0; i < 15; i++)
        {
            var isCnpj = faker.Random.Double() < 0.7;
            var document = isCnpj ? NextUniqueDocument(faker, usedDocuments, "##.###.###/####-##") : NextUniqueDocument(faker, usedDocuments, "###.###.###-##");
            var name = isCnpj ? faker.Company.CompanyName() : faker.Name.FullName();

            // Garante pelo menos 8 clientes e 3 fornecedores, como pedido no
            // milestone de cotação/pedido, o resto é papel aleatório.
            var roles = i < 8
                ? CompanyRoles.Cliente
                : i < 11
                    ? CompanyRoles.Fornecedor
                    : faker.Random.ArrayElement(roleOptions);

            if (faker.Random.Bool(0.3f))
                roles |= faker.Random.ArrayElement(roleOptions);

            var company = new Company
            {
                Name = name,
                TradeName = isCnpj && faker.Random.Bool(0.5f) ? faker.Company.CompanyName() : null,
                Document = document,
                DocumentType = isCnpj ? CompanyDocumentType.Cnpj : CompanyDocumentType.Cpf,
                StateRegistration = isCnpj ? faker.Random.ReplaceNumbers("##########") : null,
                Email = faker.Internet.Email(),
                Phone = faker.Phone.PhoneNumber("(##) #####-####"),
                Roles = roles,
                IsActive = faker.Random.Bool(0.9f),
            };

            company.Addresses.Add(new Address
            {
                Type = AddressType.Principal,
                Street = faker.Address.StreetName(),
                Number = faker.Address.BuildingNumber(),
                Complement = faker.Random.Bool(0.3f) ? faker.Address.SecondaryAddress() : null,
                District = faker.Address.County(),
                City = faker.Address.City(),
                State = faker.Address.StateAbbr(),
                ZipCode = faker.Address.ZipCode("#####-###"),
                IsDefault = true,
            });

            company.Contacts.Add(new Contact
            {
                Name = faker.Name.FullName(),
                Role = faker.Name.JobTitle(),
                Email = faker.Internet.Email(),
                Phone = faker.Phone.PhoneNumber("(##) #####-####"),
            });

            companies.Add(company);
        }

        return companies;
    }

    private static List<Vehicle> SeedVehicles()
    {
        var faker = new Faker("pt_BR");
        var usedPlates = new HashSet<string>();
        var usedRenavams = new HashSet<string>();
        var vehicles = new List<Vehicle>();

        for (var i = 0; i < 25; i++)
        {
            var manufactureYear = faker.Random.Int(2008, 2024);
            var capacity = faker.Random.Decimal(3000, 45000);

            var status = faker.Random.Double() switch
            {
                < 0.7 => VehicleStatus.Disponivel,
                < 0.8 => VehicleStatus.EmViagem,
                < 0.9 => VehicleStatus.EmManutencao,
                _ => VehicleStatus.Indisponivel,
            };

            vehicles.Add(new Vehicle
            {
                PlateNumber = NextUniquePlate(faker, usedPlates),
                Renavam = NextUniqueDigits(faker, usedRenavams, 11),
                Brand = faker.Random.ArrayElement(TruckBrands),
                Model = faker.Random.ArrayElement(TruckModels),
                ManufactureYear = manufactureYear,
                ModelYear = faker.Random.Bool(0.2f) ? manufactureYear + 1 : manufactureYear,
                Type = faker.Random.Enum<VehicleType>(),
                AxleCount = faker.Random.Int(2, 9),
                CapacityKg = Math.Round(capacity, 2),
                TareWeightKg = Math.Round(capacity * faker.Random.Decimal(0.25m, 0.4m), 2),
                Odometer = Math.Round(faker.Random.Decimal(0, 480000), 1),
                Status = status,
            });
        }

        return vehicles;
    }

    private static List<Driver> SeedDrivers(DateOnly today)
    {
        var faker = new Faker("pt_BR");
        var usedCpfs = new HashSet<string>();
        var usedCnhNumbers = new HashSet<string>();
        var drivers = new List<Driver>();

        for (var i = 0; i < 30; i++)
        {
            var name = faker.Name.FullName();

            var availability = faker.Random.Double() switch
            {
                < 0.75 => DriverAvailabilityStatus.Disponivel,
                < 0.85 => DriverAvailabilityStatus.EmViagem,
                < 0.92 => DriverAvailabilityStatus.Ferias,
                < 0.97 => DriverAvailabilityStatus.Afastado,
                _ => DriverAvailabilityStatus.Inativo,
            };

            drivers.Add(new Driver
            {
                Name = name,
                Cpf = NextUniqueDocument(faker, usedCpfs, "###.###.###-##"),
                BirthDate = DateOnly.FromDateTime(faker.Date.Between(DateTime.UtcNow.AddYears(-60), DateTime.UtcNow.AddYears(-21))),
                Phone = faker.Phone.PhoneNumber("(##) #####-####"),
                Email = faker.Internet.Email(name),
                CnhNumber = NextUniqueDigits(faker, usedCnhNumbers, 11),
                CnhCategory = faker.Random.Enum<CnhCategory>(),
                CnhExpiryDate = RandomExpiryDate(faker, today),
                HireDate = DateOnly.FromDateTime(faker.Date.Past(8, DateTime.UtcNow.AddYears(-1))),
                EmploymentType = faker.Random.Enum<EmploymentType>(),
                AvailabilityStatus = availability,
            });
        }

        return drivers;
    }

    private static List<Document> SeedDocuments(List<Company> companies, List<Vehicle> vehicles, List<Driver> drivers, DateOnly today, int expiringSoonThresholdDays)
    {
        var faker = new Faker("pt_BR");
        var documents = new List<Document>();

        // Uma CNH por motorista, sempre com o mesmo número/validade já
        // cadastrados no motorista -- não faz sentido divergir.
        foreach (var driver in drivers)
        {
            documents.Add(BuildDocument(
                DocumentOwnerType.Driver,
                driver.Id,
                DocumentType.Cnh,
                driver.CnhNumber,
                driver.CnhExpiryDate.AddYears(-5),
                driver.CnhExpiryDate,
                today,
                expiringSoonThresholdDays));
        }

        // CRLV / apólice / licenciamento pra uma amostra dos veículos.
        var vehicleDocTypes = new[] { DocumentType.Crlv, DocumentType.ApoliceSeguro, DocumentType.Licenciamento, DocumentType.Antt };
        foreach (var vehicle in faker.PickRandom(vehicles, 6))
        {
            var expiry = RandomExpiryDate(faker, today);
            documents.Add(BuildDocument(
                DocumentOwnerType.Vehicle,
                vehicle.Id,
                faker.Random.ArrayElement(vehicleDocTypes),
                faker.Random.ReplaceNumbers("##########"),
                expiry.AddYears(-1),
                expiry,
                today,
                expiringSoonThresholdDays));
        }

        // Contrato de transporte pra uma amostra das empresas.
        foreach (var company in faker.PickRandom(companies, 4))
        {
            var expiry = RandomExpiryDate(faker, today);
            documents.Add(BuildDocument(
                DocumentOwnerType.Company,
                company.Id,
                DocumentType.ContratoTransporte,
                faker.Random.ReplaceNumbers("CT-#####/####"),
                expiry.AddYears(-2),
                expiry,
                today,
                expiringSoonThresholdDays));
        }

        return documents;
    }

    private static Document BuildDocument(DocumentOwnerType ownerType, Guid ownerId, DocumentType type, string number, DateOnly issueDate, DateOnly expiryDate, DateOnly today, int expiringSoonThresholdDays)
    {
        var document = new Document
        {
            OwnerType = ownerType,
            OwnerId = ownerId,
            Type = type,
            Number = number,
            IssueDate = issueDate,
            ExpiryDate = expiryDate,
        };

        document.RecomputeStatus(today, expiringSoonThresholdDays);
        return document;
    }

    // Espalha os vencimentos como pedido: 80% válido, 10% próximo (dentro
    // dos próximos 29 dias), 10% vencido -- pra ter variedade real pras
    // regras de negócio de CNH/documentos vencidos.
    private static DateOnly RandomExpiryDate(Faker faker, DateOnly today)
    {
        var roll = faker.Random.Double();
        if (roll < 0.10)
            return today.AddDays(-faker.Random.Int(1, 400));
        if (roll < 0.20)
            return today.AddDays(faker.Random.Int(1, 29));
        return today.AddDays(faker.Random.Int(60, 900));
    }

    private static string NextUniquePlate(Faker faker, HashSet<string> used)
    {
        string plate;
        do
        {
            plate = $"{faker.Random.String2(3, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")}-{faker.Random.Number(1000, 9999)}";
        } while (!used.Add(plate));

        return plate;
    }

    private static string NextUniqueDigits(Faker faker, HashSet<string> used, int length)
    {
        string value;
        do
        {
            value = faker.Random.ReplaceNumbers(new string('#', length));
        } while (!used.Add(value));

        return value;
    }

    private static string NextUniqueDocument(Faker faker, HashSet<string> used, string mask)
    {
        string value;
        do
        {
            value = faker.Random.ReplaceNumbers(mask);
        } while (!used.Add(value));

        return value;
    }
}
