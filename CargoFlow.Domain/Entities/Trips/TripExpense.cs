using CargoFlow.Domain.Common;

namespace CargoFlow.Domain.Entities.Trips;

public enum TripExpenseType
{
    Pedagio,
    Diaria,
    Manutencao,
    Diversos,
}

// Combustível não entra aqui -- vem de Fueling (vinculado por TripId),
// evita contar o mesmo custo duas vezes na rentabilidade.
public class TripExpense : Entity
{
    public Guid TripId { get; set; }
    public Trip? Trip { get; set; }

    public TripExpenseType Type { get; set; }
    public string? Description { get; set; }
    public decimal Value { get; set; }
    public DateOnly ExpenseDate { get; set; }
}
