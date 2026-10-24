namespace FarmacosAltamira.Domain.Entities;

public sealed class RecepcionMercancia
{
    public int IdRecepcion { get; set; }
    public int IdUbicacion { get; set; }
    public int IdUsuario { get; set; }
    public DateTimeOffset FechaHora { get; set; }
    public string? DocumentoReferencia { get; set; }
    public string? Observaciones { get; set; }

    public Ubicacion Ubicacion { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public ICollection<DetalleRecepcion> Detalles { get; set; } = new List<DetalleRecepcion>();
}