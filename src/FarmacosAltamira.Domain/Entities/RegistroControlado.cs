namespace FarmacosAltamira.Domain.Entities;

public sealed class RegistroControlado
{
    public long IdRegistro { get; set; }
    public int IdReceta { get; set; }
    public int IdDetalle { get; set; }
    public int IdProducto { get; set; }
    public int IdLote { get; set; }
    public int IdUsuario { get; set; }
    public DateTimeOffset FechaHora { get; set; }
    public int CantidadSurtida { get; set; }
    public string NombrePaciente { get; set; } = string.Empty;
    public int ExistenciaPosterior { get; set; }

    public Receta Receta { get; set; } = null!;
    public DetalleVenta DetalleVenta { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
    public Lote Lote { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}