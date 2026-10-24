namespace FarmacosAltamira.Domain.Entities;

public sealed class DetalleDevolucion
{
    public int IdDevolucion { get; set; }
    public int IdDetalle { get; set; }
    public int Cantidad { get; set; }
    public bool ReingresaInventario { get; set; } = true;

    public Devolucion Devolucion { get; set; } = null!;
    public DetalleVenta DetalleVenta { get; set; } = null!;
}