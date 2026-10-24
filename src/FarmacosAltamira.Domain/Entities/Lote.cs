using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class Lote
{
    public int IdLote { get; set; }
    public int IdProducto { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public DateOnly FechaCaducidad { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public EstadoLote Estado { get; set; } = EstadoLote.Vigente;

    public Producto Producto { get; set; } = null!;
    public ICollection<Existencia> Existencias { get; set; } = new List<Existencia>();
}