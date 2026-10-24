using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Application.Common.Interfaces;

public interface IAuditoria
{
    Task RegistrarAsync(
        int idUsuario,
        int? idUbicacion,
        string modulo,
        string accion,
        string? entidad = null,
        string? idEntidad = null,
        string? detalle = null,
        string? ipOrigen = null,
        CancellationToken cancellationToken = default);
}