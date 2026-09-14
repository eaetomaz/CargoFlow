using CargoFlow.Domain.Entities.Documents;

namespace CargoFlow.Application.Tests.Documents;

public class DocumentStatusTests
{
    private const int ThresholdDays = 30;
    private static readonly DateOnly Today = new(2026, 9, 9);

    private static Document DocumentExpiringOn(DateOnly expiryDate) => new()
    {
        Type = DocumentType.Cnh,
        Number = "12345",
        IssueDate = expiryDate.AddYears(-5),
        ExpiryDate = expiryDate,
    };

    [Fact]
    public void RecomputeStatus_ExpiryInThePast_IsVencido()
    {
        var document = DocumentExpiringOn(Today.AddDays(-1));
        document.RecomputeStatus(Today, ThresholdDays);
        Assert.Equal(DocumentStatus.Vencido, document.Status);
    }

    [Fact]
    public void RecomputeStatus_ExpiryFarInThePast_IsVencido()
    {
        var document = DocumentExpiringOn(Today.AddYears(-1));
        document.RecomputeStatus(Today, ThresholdDays);
        Assert.Equal(DocumentStatus.Vencido, document.Status);
    }

    [Fact]
    public void RecomputeStatus_ExpiryToday_IsProximoVencimento()
    {
        // Hoje ainda não é "vencido" (ExpiryDate < today é falso), mas já
        // cai dentro da janela de alerta -- comportamento documentado, não
        // acidental.
        var document = DocumentExpiringOn(Today);
        document.RecomputeStatus(Today, ThresholdDays);
        Assert.Equal(DocumentStatus.ProximoVencimento, document.Status);
    }

    [Fact]
    public void RecomputeStatus_ExpiryExactlyAtThresholdBoundary_IsProximoVencimento()
    {
        var document = DocumentExpiringOn(Today.AddDays(ThresholdDays));
        document.RecomputeStatus(Today, ThresholdDays);
        Assert.Equal(DocumentStatus.ProximoVencimento, document.Status);
    }

    [Fact]
    public void RecomputeStatus_ExpiryOneDayPastThreshold_IsValido()
    {
        var document = DocumentExpiringOn(Today.AddDays(ThresholdDays + 1));
        document.RecomputeStatus(Today, ThresholdDays);
        Assert.Equal(DocumentStatus.Valido, document.Status);
    }

    [Fact]
    public void RecomputeStatus_ExpiryFarInTheFuture_IsValido()
    {
        var document = DocumentExpiringOn(Today.AddYears(2));
        document.RecomputeStatus(Today, ThresholdDays);
        Assert.Equal(DocumentStatus.Valido, document.Status);
    }

    [Fact]
    public void RecomputeStatus_TransitionsBackToValido_WhenReissuedWithLaterExpiry()
    {
        // Simula reemissão do documento: era Vencido, ganha nova validade
        // distante -- o status precisa refletir a mudança, não ficar preso.
        var document = DocumentExpiringOn(Today.AddDays(-10));
        document.RecomputeStatus(Today, ThresholdDays);
        Assert.Equal(DocumentStatus.Vencido, document.Status);

        document.ExpiryDate = Today.AddYears(1);
        document.RecomputeStatus(Today, ThresholdDays);
        Assert.Equal(DocumentStatus.Valido, document.Status);
    }
}
