namespace FarmacosAltamira.Domain.Entities;

public sealed class Devolucion
{
    public int IdDevolucion { get; set; }
    public int IdVenta { get; set; }
    public int IdUsuario { get; set; }
    public string Folio { get; set; } = string.Empty;
    public DateTimeOffset Fecha { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal MontoReembolsado { get; set; }
    public string? FormaReembolso { get; set; }

    public Venta Venta { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public ICollection<DetalleDevolucion> Detalles { get; set; } = new List<DetalleDevolucion>();
}