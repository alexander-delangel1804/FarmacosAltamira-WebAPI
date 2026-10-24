namespace FarmacosAltamira.Domain.Entities;

public sealed class DetalleTraslado
{
    public int IdTraslado { get; set; }
    public int IdLote { get; set; }
    public int CantidadEnviada { get; set; }
    public int? CantidadRecibida { get; set; }

    public Traslado Traslado { get; set; } = null!;
    public Lote Lote { get; set; } = null!;
}