namespace FarmacosAltamira.Domain.Entities;

public sealed class Producto
{
    public int IdProducto { get; set; }
    public int IdCategoria { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;
    public string? SustanciaActiva { get; set; }
    public string? Presentacion { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal TasaIva { get; set; }
    public int StockMinimo { get; set; }
    public bool EsControlado { get; set; }
    public bool Activo { get; set; } = true;

    public Categoria Categoria { get; set; } = null!;
    public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
    public ICollection<StockMinimoUbicacion> StockMinimoUbicaciones { get; set; } = new List<StockMinimoUbicacion>();
    public ProductoFiscal? DatosFiscales { get; set; }
}