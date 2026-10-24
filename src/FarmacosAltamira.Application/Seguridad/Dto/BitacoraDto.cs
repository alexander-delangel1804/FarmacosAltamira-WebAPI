using FarmacosAltamira.Contracts;

namespace FarmacosAltamira.Application.Seguridad.Dto;

public sealed record BitacoraDto(
    long IdEvento,
    int IdUsuario,
    string NombreUsuario,
    int? IdUbicacion,
    string? UbicacionNombre,
    DateTimeOffset FechaHora,
    string Modulo,
    string Accion,
    string? Entidad,
    string? IdEntidad,
    string? Detalle,
    string? IpOrigen);

public record BitacoraFiltroDto : PaginationRequest
{
    public int? IdUsuario { get; init; }
    public int? IdUbicacion { get; init; }
    public string? Modulo { get; init; }
    public string? Accion { get; init; }
    public DateTimeOffset? FechaDesde { get; init; }
    public DateTimeOffset? FechaHasta { get; init; }
}