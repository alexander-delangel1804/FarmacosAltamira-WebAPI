using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class Venta
{
    public int IdVenta { get; set; }
    public int IdCorte { get; set; }
    public int IdUsuario { get; set; }
    public string Folio { get; set; } = string.Empty;
    public Guid UuidOperacion { get; set; }
    public DateTimeOffset FechaHora { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public EstadoVenta Estado { get; set; } = EstadoVenta.EnCaptura;
    public int IdUbicacion { get; set; }
    public bool RequiereFactura { get; set; }
    public bool Sincronizada { get; set; } = true;
    public DateTimeOffset? FechaSincronizacion { get; set; }
    public DateTimeOffset? FechaRecepcion { get; set; }

    public CorteCaja Corte { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public Devolucion? Devolucion { get; set; }
    public CancelacionVenta? Cancelacion { get; set; }
    public ComprobanteFiscal? Comprobante { get; set; }
}