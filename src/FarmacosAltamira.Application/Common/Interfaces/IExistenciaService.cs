namespace FarmacosAltamira.Application.Common.Interfaces;

public interface IExistenciaService
{
    Task<int> ObtenerDisponibleAsync(int idLote, int idUbicacion, CancellationToken ct);
    Task<bool> ReservarAsync(int idLote, int idUbicacion, int cantidad, CancellationToken ct);
    Task LiberarReservaAsync(int idLote, int idUbicacion, int cantidad, CancellationToken ct);
    Task<bool> DescontarAsync(int idLote, int idUbicacion, int cantidad, CancellationToken ct);
    Task SumarAsync(int idLote, int idUbicacion, int cantidad, CancellationToken ct);
}