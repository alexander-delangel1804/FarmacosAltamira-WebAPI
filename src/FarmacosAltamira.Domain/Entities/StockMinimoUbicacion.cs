namespace FarmacosAltamira.Domain.Entities;

public sealed class StockMinimoUbicacion
{
    public int IdProducto { get; set; }
    public int IdUbicacion { get; set; }
    public int StockMinimo { get; set; }

    public Producto Producto { get; set; } = null!;
    public Ubicacion Ubicacion { get; set; } = null!;
}