using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class Traslado
{
    public int IdTraslado { get; set; }
    public string Folio { get; set; } = string.Empty;
    public int IdOrigen { get; set; }
    public int IdDestino { get; set; }
    public int IdUsuarioSolicita { get; set; }
    public DateTimeOffset FechaSolicitud { get; set; }
    public DateTimeOffset? FechaEnvio { get; set; }
    public DateTimeOffset? FechaRecepcion { get; set; }
    public EstadoTraslado Estado { get; set; } = EstadoTraslado.Solicitado;
    public string? Observaciones { get; set; }
    public uint Xmin { get; set; }

    public Ubicacion Origen { get; set; } = null!;
    public Ubicacion Destino { get; set; } = null!;
    public Usuario UsuarioSolicita { get; set; } = null!;
    public ICollection<DetalleTraslado> Detalles { get; set; } = new List<DetalleTraslado>();
}