namespace FarmacosAltamira.Domain.Entities.Local;

public sealed class PagoLocal
{
    public int IdPagoLocal { get; set; }
    public int IdLocal { get; set; }
    public string FormaPago { get; set; } = string.Empty; // EFECTIVO, TARJETA_DEBITO, TARJETA_CREDITO, TRANSFERENCIA
    public decimal Monto { get; set; }
    public string? Referencia { get; set; }
    public decimal Cambio { get; set; }
}