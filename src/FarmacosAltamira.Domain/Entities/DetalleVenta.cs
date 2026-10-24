namespace FarmacosAltamira.Domain.Entities;

public sealed class DetalleVenta
{
    public int IdDetalle { get; set; }
    public int IdVenta { get; set; }
    public int IdProducto { get; set; }
    public int IdLote { get; set; }
    public int? IdPromocion { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal Importe { get; set; }

    public Venta Venta { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
    public Lote Lote { get; set; } = null!;
    public Promocion? Promocion { get; set; }
    public RegistroControlado? RegistroControlado { get; set; }
}