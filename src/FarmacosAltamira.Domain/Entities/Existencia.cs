namespace FarmacosAltamira.Domain.Entities;

public sealed class Existencia
{
    public int IdLote { get; set; }
    public int IdUbicacion { get; set; }
    public int Cantidad { get; set; }
    public int CantidadReservada { get; set; }

    public Lote Lote { get; set; } = null!;
    public Ubicacion Ubicacion { get; set; } = null!;

    public int Disponible => Cantidad - CantidadReservada;
}