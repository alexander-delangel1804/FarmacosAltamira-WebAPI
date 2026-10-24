using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Application.Common.Interfaces;

public interface IKardexService
{
    Task MoverAsync(
        int idLote,
        int idUbicacion,
        int cantidad,
        TipoMovimientoInventario tipo,
        int idUsuario,
        string? motivo,
        string? documentoOrigen,
        long? idDocumento,
        CancellationToken cancellationToken = default);
}