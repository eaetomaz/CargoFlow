using CargoFlow.Domain.Common;

namespace CargoFlow.Domain.Entities.Fleet;

public enum VehicleType
{
    Cavalo,
    CaminhaoToco,
    CaminhaoTruck,
    Carreta,
    Bitrem,
    Rodotrem,
}

public enum VehicleStatus
{
    Disponivel,
    EmViagem,
    EmManutencao,
    Indisponivel,
}

public class Vehicle : Entity, IAuditable
{
    public string PlateNumber { get; set; } = string.Empty;
    public string Renavam { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int ManufactureYear { get; set; }
    public int ModelYear { get; set; }
    public VehicleType Type { get; set; }
    public int AxleCount { get; set; }
    public decimal CapacityKg { get; set; }
    public decimal TareWeightKg { get; set; }
    public decimal Odometer { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Disponivel;
}
