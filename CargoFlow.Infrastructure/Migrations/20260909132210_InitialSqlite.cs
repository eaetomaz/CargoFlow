using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    EntityName = table.Column<string>(type: "TEXT", nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Operation = table.Column<int>(type: "INTEGER", nullable: false),
                    OldValuesJson = table.Column<string>(type: "TEXT", nullable: true),
                    NewValuesJson = table.Column<string>(type: "TEXT", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "TEXT", nullable: true),
                    Document = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DocumentType = table.Column<int>(type: "INTEGER", nullable: false),
                    StateRegistration = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Roles = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerType = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    CnhNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CnhCategory = table.Column<int>(type: "INTEGER", nullable: false),
                    CnhExpiryDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    HireDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EmploymentType = table.Column<int>(type: "INTEGER", nullable: false),
                    AvailabilityStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FreightPricingRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VehicleType = table.Column<int>(type: "INTEGER", nullable: false),
                    CargoType = table.Column<int>(type: "INTEGER", nullable: true),
                    PricePerKg = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    PricePerKm = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    TollPerKm = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    AdValoremPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    GrisPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    MinimumFreightValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreightPricingRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    DriverId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlateNumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Renavam = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Brand = table.Column<string>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: false),
                    ManufactureYear = table.Column<int>(type: "INTEGER", nullable: false),
                    ModelYear = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    AxleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CapacityKg = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TareWeightKg = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Odometer = table.Column<decimal>(type: "numeric(10,1)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountsPayable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    SupplierCompanyId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceType = table.Column<string>(type: "TEXT", nullable: true),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PaidDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountsPayable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountsPayable_Companies_SupplierCompanyId",
                        column: x => x.SupplierCompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountsReceivable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerCompanyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TripId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TransportOrderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ReceivedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountsReceivable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountsReceivable_Companies_CustomerCompanyId",
                        column: x => x.CustomerCompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompanyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Street = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<string>(type: "TEXT", nullable: false),
                    Complement = table.Column<string>(type: "TEXT", nullable: true),
                    District = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<string>(type: "TEXT", nullable: false),
                    ZipCode = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompanyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FreightQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerCompanyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginCity = table.Column<string>(type: "TEXT", nullable: false),
                    OriginState = table.Column<string>(type: "TEXT", nullable: false),
                    DestinationCity = table.Column<string>(type: "TEXT", nullable: false),
                    DestinationState = table.Column<string>(type: "TEXT", nullable: false),
                    CargoType = table.Column<int>(type: "INTEGER", nullable: false),
                    CargoWeightKg = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CargoValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    RequiredVehicleType = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedDistanceKm = table.Column<decimal>(type: "numeric(10,1)", nullable: false),
                    FreightWeightValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    TollValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    AdValoremValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    GrisValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    OtherCostsValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    TotalValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreightQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FreightQuotes_Companies_CustomerCompanyId",
                        column: x => x.CustomerCompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransportOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FreightQuoteId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerCompanyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShipperCompanyId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ConsigneeCompanyId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OriginCity = table.Column<string>(type: "TEXT", nullable: false),
                    OriginState = table.Column<string>(type: "TEXT", nullable: false),
                    DestinationCity = table.Column<string>(type: "TEXT", nullable: false),
                    DestinationState = table.Column<string>(type: "TEXT", nullable: false),
                    CargoDescription = table.Column<string>(type: "TEXT", nullable: false),
                    CargoWeightKg = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CargoValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    FreightValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedPickupDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RequestedDeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransportOrders_Companies_CustomerCompanyId",
                        column: x => x.CustomerCompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fuelings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VehicleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DriverId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TripId = table.Column<Guid>(type: "TEXT", nullable: true),
                    GasStationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FuelingDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    LiterQuantity = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    OdometerReading = table.Column<decimal>(type: "numeric(10,1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fuelings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fuelings_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fuelings_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VehicleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OdometerAtService = table.Column<decimal>(type: "numeric(10,1)", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    ServiceProvider = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceOrders_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Occurrences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TripId = table.Column<Guid>(type: "TEXT", nullable: true),
                    VehicleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DriverId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Occurrences_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Occurrences_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransportOrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VehicleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrailerVehicleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DriverId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginCity = table.Column<string>(type: "TEXT", nullable: false),
                    OriginState = table.Column<string>(type: "TEXT", nullable: false),
                    DestinationCity = table.Column<string>(type: "TEXT", nullable: false),
                    DestinationState = table.Column<string>(type: "TEXT", nullable: false),
                    ScheduledDepartureAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualDepartureAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedArrivalAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualArrivalAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlannedDistanceKm = table.Column<decimal>(type: "numeric(10,1)", nullable: false),
                    ActualDistanceKm = table.Column<decimal>(type: "numeric(10,1)", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    FreightRevenue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trips_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trips_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OccurrenceAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OccurrenceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccurrenceAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OccurrenceAttachments_Occurrences_OccurrenceId",
                        column: x => x.OccurrenceId,
                        principalTable: "Occurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TripEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TripId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripEvents_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TripExpenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TripId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ExpenseDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripExpenses_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayable_Status",
                table: "AccountsPayable",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayable_SupplierCompanyId",
                table: "AccountsPayable",
                column: "SupplierCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivable_CustomerCompanyId",
                table: "AccountsReceivable",
                column: "CustomerCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivable_Status",
                table: "AccountsReceivable",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivable_TripId",
                table: "AccountsReceivable",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CompanyId",
                table: "Addresses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Document",
                table: "Companies",
                column: "Document",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_CompanyId",
                table: "Contacts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_OwnerType_OwnerId",
                table: "Documents",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Status",
                table: "Documents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CnhNumber",
                table: "Drivers",
                column: "CnhNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_Cpf",
                table: "Drivers",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FreightQuotes_CustomerCompanyId",
                table: "FreightQuotes",
                column: "CustomerCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_FreightQuotes_Status",
                table: "FreightQuotes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Fuelings_DriverId",
                table: "Fuelings",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Fuelings_TripId",
                table: "Fuelings",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Fuelings_VehicleId",
                table: "Fuelings",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrders_Status",
                table: "MaintenanceOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrders_VehicleId",
                table: "MaintenanceOrders",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_OccurrenceAttachments_OccurrenceId",
                table: "OccurrenceAttachments",
                column: "OccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Occurrences_DriverId",
                table: "Occurrences",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Occurrences_Status",
                table: "Occurrences",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Occurrences_TripId",
                table: "Occurrences",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Occurrences_VehicleId",
                table: "Occurrences",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportOrders_CustomerCompanyId",
                table: "TransportOrders",
                column: "CustomerCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportOrders_Status",
                table: "TransportOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TripEvents_TripId",
                table: "TripEvents",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_TripExpenses_TripId",
                table: "TripExpenses",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_DriverId",
                table: "Trips",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_Status",
                table: "Trips",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_VehicleId",
                table: "Trips",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_PlateNumber",
                table: "Vehicles",
                column: "PlateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Renavam",
                table: "Vehicles",
                column: "Renavam",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountsPayable");

            migrationBuilder.DropTable(
                name: "AccountsReceivable");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "FreightPricingRules");

            migrationBuilder.DropTable(
                name: "FreightQuotes");

            migrationBuilder.DropTable(
                name: "Fuelings");

            migrationBuilder.DropTable(
                name: "MaintenanceOrders");

            migrationBuilder.DropTable(
                name: "OccurrenceAttachments");

            migrationBuilder.DropTable(
                name: "TransportOrders");

            migrationBuilder.DropTable(
                name: "TripEvents");

            migrationBuilder.DropTable(
                name: "TripExpenses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Occurrences");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "Vehicles");
        }
    }
}
