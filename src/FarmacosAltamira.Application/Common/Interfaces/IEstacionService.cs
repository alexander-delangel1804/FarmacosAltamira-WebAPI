namespace FarmacosAltamira.Application.Common.Interfaces;

public interface IEstacionService
{
    Task<bool> ValidarEstacionAsync(int idEstacion, string secreto, CancellationToken ct);
    Task<int?> ObtenerIdUbicacionPorEstacionAsync(int idEstacion, CancellationToken ct);
    Task<int?> ObtenerIdCajaPorEstacionAsync(int idEstacion, CancellationToken ct);
    Task ActualizarUltimaConexionAsync(int idEstacion, string versionCliente, CancellationToken ct);
}