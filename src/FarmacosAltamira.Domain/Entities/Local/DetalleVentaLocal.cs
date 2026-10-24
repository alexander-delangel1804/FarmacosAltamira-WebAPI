namespace FarmacosAltamira.Domain.Entities.Local;

public sealed class DetalleVentaLocal
{
    public int IdDetalleLocal { get; set; }
    public int IdLocal { get; set; }
    public int IdProducto { get; set; }
    public int IdLote { get; set; }
    public int? IdPromocion { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal Importe { get; set; }
}