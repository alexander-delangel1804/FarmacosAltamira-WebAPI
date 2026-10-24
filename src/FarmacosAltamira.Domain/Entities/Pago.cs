using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class Pago
{
    public int IdPago { get; set; }
    public int IdVenta { get; set; }
    public FormaPago FormaPago { get; set; }
    public decimal Monto { get; set; }
    public string? Referencia { get; set; }
    public decimal Cambio { get; set; }

    public Venta Venta { get; set; } = null!;
}