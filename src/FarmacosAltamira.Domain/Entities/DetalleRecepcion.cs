namespace FarmacosAltamira.Domain.Entities;

public sealed class DetalleRecepcion
{
    public int IdRecepcion { get; set; }
    public int IdLote { get; set; }
    public int CantidadRecibida { get; set; }

    public RecepcionMercancia Recepcion { get; set; } = null!;
    public Lote Lote { get; set; } = null!;
}