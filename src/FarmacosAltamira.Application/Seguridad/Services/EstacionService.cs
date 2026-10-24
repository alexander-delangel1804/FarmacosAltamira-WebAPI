using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Seguridad.Services;

public sealed class EstacionService : IEstacionService
{
    private readonly ICentralDbContext _db;
    private readonly IPasswordHasher _hasher;

    public EstacionService(ICentralDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<bool> ValidarEstacionAsync(int idEstacion, string secreto, CancellationToken ct)
    {
        var estacion = await _db.Estaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEstacion == idEstacion && e.Activa, ct);

        if (estacion == null) return false;

        return _hasher.Verify(secreto, estacion.HashSecreto);
    }

    public async Task<int?> ObtenerIdUbicacionPorEstacionAsync(int idEstacion, CancellationToken ct)
    {
        return await _db.Estaciones
            .AsNoTracking()
            .Where(e => e.IdEstacion == idEstacion && e.Activa)
            .Select(e => e.IdUbicacion)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int?> ObtenerIdCajaPorEstacionAsync(int idEstacion, CancellationToken ct)
    {
        return await _db.Estaciones
            .AsNoTracking()
            .Where(e => e.IdEstacion == idEstacion && e.Activa)
            .Select(e => e.IdCaja)
            .FirstOrDefaultAsync(ct);
    }

    public async Task ActualizarUltimaConexionAsync(int idEstacion, string versionCliente, CancellationToken ct)
    {
        var estacion = await _db.Estaciones.FirstOrDefaultAsync(e => e.IdEstacion == idEstacion, ct);
        if (estacion != null)
        {
            estacion.UltimaConexion = DateTimeOffset.UtcNow;
            estacion.VersionCliente = versionCliente;
            await _db.SaveChangesAsync(ct);
        }
    }
}