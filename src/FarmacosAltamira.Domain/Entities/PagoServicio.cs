using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class PagoServicio
{
    public int IdPagoServicio { get; set; }
    public int IdServicio { get; set; }
    public int IdUbicacion { get; set; }
    public int IdUsuario { get; set; }
    public int IdCorte { get; set; }
    public string Folio { get; set; } = string.Empty;
    public DateTimeOffset FechaHora { get; set; }
    public string ReferenciaCliente { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal Comision { get; set; }
    public decimal TotalCobrado { get; set; }
    public string? FolioAutorizacion { get; set; }
    public EstatusPagoServicio Estatus { get; set; } = EstatusPagoServicio.Pendiente;
    public Guid? UuidOperacion { get; set; }
    public int Intentos { get; set; }
    public DateTimeOffset? FechaRespuesta { get; set; }
    public string? CodigoRespuesta { get; set; }

    public CatalogoServicio Servicio { get; set; } = null!;
    public Ubicacion Ubicacion { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public CorteCaja Corte { get; set; } = null!;
    public ICollection<PagoServicioEvento> Eventos { get; set; } = new List<PagoServicioEvento>();
}