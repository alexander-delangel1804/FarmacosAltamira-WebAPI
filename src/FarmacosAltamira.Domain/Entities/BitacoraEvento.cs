using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class BitacoraEvento
{
    public long IdEvento { get; set; }
    public int IdUsuario { get; set; }
    public int? IdUbicacion { get; set; }
    public DateTimeOffset FechaHora { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string? Entidad { get; set; }
    public string? IdEntidad { get; set; }
    public string? Detalle { get; set; }
    public string? IpOrigen { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Ubicacion? Ubicacion { get; set; }
}