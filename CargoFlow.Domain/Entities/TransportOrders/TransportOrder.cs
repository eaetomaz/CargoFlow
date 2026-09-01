using CargoFlow.Domain.Common;
using CargoFlow.Domain.Entities.Companies;

namespace CargoFlow.Domain.Entities.TransportOrders;

public enum TransportOrderStatus
{
    Criada,
    Programada,
    EmTransporte,
    Entregue,
    Faturada,
    Cancelada,
}

public class TransportOrder : Entity, IAuditable
{
    public Guid? FreightQuoteId { get; set; }

    public Guid CustomerCompanyId { get; set; }
    public Company? CustomerCompany { get; set; }
    public Guid? ShipperCompanyId { get; set; }
    public Guid? ConsigneeCompanyId { get; set; }

    public string OriginCity { get; set; } = string.Empty;
    public string OriginState { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public string DestinationState { get; set; } = string.Empty;

    public string CargoDescription { get; set; } = string.Empty;
    public decimal CargoWeightKg { get; set; }
    public decimal CargoValue { get; set; }
    public decimal FreightValue { get; set; }

    public TransportOrderStatus Status { get; set; } = TransportOrderStatus.Criada;

    public DateTime RequestedPickupDate { get; set; }
    public DateTime RequestedDeliveryDate { get; set; }

    // Guards espelham a regra 5 do documento de contexto ("viagem encerrada
    // não recebe determinadas alterações") aplicada à OT: cada transição só
    // é aceita a partir do estado anterior esperado do fluxo Criada ->
    // Programada -> EmTransporte -> Entregue -> Faturada.
    public void MarkScheduled()
    {
        if (Status != TransportOrderStatus.Criada)
            throw new InvalidOperationException($"Só é possível programar uma OT \"Criada\" -- esta está \"{Status}\".");
        Status = TransportOrderStatus.Programada;
        UpdateTimestamp();
    }

    public void MarkInTransit()
    {
        if (Status != TransportOrderStatus.Programada)
            throw new InvalidOperationException($"Só é possível iniciar transporte de uma OT \"Programada\" -- esta está \"{Status}\".");
        Status = TransportOrderStatus.EmTransporte;
        UpdateTimestamp();
    }

    public void MarkDelivered()
    {
        if (Status != TransportOrderStatus.EmTransporte)
            throw new InvalidOperationException($"Só é possível entregar uma OT \"EmTransporte\" -- esta está \"{Status}\".");
        Status = TransportOrderStatus.Entregue;
        UpdateTimestamp();
    }

    public void MarkBilled()
    {
        if (Status != TransportOrderStatus.Entregue)
            throw new InvalidOperationException($"Só é possível faturar uma OT \"Entregue\" -- esta está \"{Status}\".");
        Status = TransportOrderStatus.Faturada;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status is TransportOrderStatus.Entregue or TransportOrderStatus.Faturada)
            throw new InvalidOperationException($"OT \"{Status}\" não pode mais ser cancelada.");
        Status = TransportOrderStatus.Cancelada;
        UpdateTimestamp();
    }
}
