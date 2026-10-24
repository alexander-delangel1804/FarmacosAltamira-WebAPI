namespace FarmacosAltamira.Domain.Entities.Local;

public sealed class LoteLocal
{
    public int IdLote { get; set; }
    public int IdProducto { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public string FechaCaducidad { get; set; } = string.Empty; // ISO8601 date string
}