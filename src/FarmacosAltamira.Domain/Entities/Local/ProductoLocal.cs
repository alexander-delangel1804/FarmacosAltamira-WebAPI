namespace FarmacosAltamira.Domain.Entities.Local;

public sealed class ProductoLocal
{
    public int IdProducto { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;
    public decimal PrecioVenta { get; set; }
    public decimal TasaIva { get; set; }
    public bool EsControlado { get; set; }
    public bool Activo { get; set; }
}