namespace FarmacosAltamira.Domain.Entities;

public sealed class ProductoFiscal
{
    public int IdProducto { get; set; }
    public string ClaveProdServ { get; set; } = string.Empty;
    public string ClaveUnidad { get; set; } = string.Empty;
    public string ObjetoImp { get; set; } = "02";
    public decimal? IepsTasa { get; set; }

    public Producto Producto { get; set; } = null!;
}