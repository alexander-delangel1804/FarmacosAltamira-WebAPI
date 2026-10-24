using FarmacosAltamira.Contracts;

namespace FarmacosAltamira.Application.Seguridad.Dto;

public sealed record RespaldoDto(
    int IdRespaldo,
    int? IdUsuario,
    string? UsuarioNombre,
    DateTimeOffset FechaHora,
    string Tipo,
    string Ruta,
    decimal? TamanoMb,
    bool Cifrado,
    string Estado,
    bool Verificado);

public record RespaldoFiltroDto : PaginationRequest
{
    public int? IdUsuario { get; init; }
    public string? Tipo { get; init; }
    public string? Estado { get; init; }
    public DateTimeOffset? FechaDesde { get; init; }
    public DateTimeOffset? FechaHasta { get; init; }
}