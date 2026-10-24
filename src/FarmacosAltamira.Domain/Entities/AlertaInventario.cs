using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class AlertaInventario
{
    public int IdAlerta { get; set; }
    public int IdProducto { get; set; }
    public int IdUbicacion { get; set; }
    public int? IdLote { get; set; }
    public TipoAlertaInventario Tipo { get; set; }
    public DateTimeOffset FechaGeneracion { get; set; }
    public string? Mensaje { get; set; }
    public bool Atendida { get; set; } = false;

    public Producto Producto { get; set; } = null!;
    public Ubicacion Ubicacion { get; set; } = null!;
    public Lote? Lote { get; set; }
}