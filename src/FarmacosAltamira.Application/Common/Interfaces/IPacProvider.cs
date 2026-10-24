using FarmacosAltamira.Domain.ValueObjects;

namespace FarmacosAltamira.Application.Common.Interfaces;

public interface IPacProvider
{
    Task<ResultadoPac> TimbrarAsync(SolicitudTimbrado solicitud, CancellationToken ct);
    Task<ResultadoPac> ConsultarAsync(string referencia, CancellationToken ct);
    Task<ResultadoPac> CancelarAsync(SolicitudCancelacion solicitud, CancellationToken ct);
}

public sealed record SolicitudCancelacion(
    Guid UuidCfdi,
    string Motivo,
    string? FolioSustituto);