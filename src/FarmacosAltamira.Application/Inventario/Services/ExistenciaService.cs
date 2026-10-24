using FarmacosAltamira.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Inventario.Services;

public sealed class ExistenciaService : IExistenciaService
{
    private readonly ICentralDbContext _db;

    public ExistenciaService(ICentralDbContext db)
    {
        _db = db;
    }

    public async Task<int> ObtenerDisponibleAsync(int idLote, int idUbicacion, CancellationToken ct)
    {
        var existencia = await _db.Existencias
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdLote == idLote && e.IdUbicacion == idUbicacion, ct);

        return existencia?.Disponible ?? 0;
    }

    public async Task<bool> ReservarAsync(int idLote, int idUbicacion, int cantidad, CancellationToken ct)
    {
        var sql = @"
            UPDATE existencia
            SET cantidad_reservada = cantidad_reservada + {0}
            WHERE id_lote = {1} AND id_ubicacion = {2}
            AND (cantidad - cantidad_reservada) >= {0}
            RETURNING cantidad_reservada;";

        var resultado = await _db.Database
            .SqlQueryRaw<int>(sql, cantidad, idLote, idUbicacion)
            .FirstOrDefaultAsync(ct);

        return resultado > 0;
    }

    public async Task LiberarReservaAsync(int idLote, int idUbicacion, int cantidad, CancellationToken ct)
    {
        var sql = @"
            UPDATE existencia
            SET cantidad_reservada = cantidad_reservada - {0}
            WHERE id_lote = {1} AND id_ubicacion = {2}
            AND cantidad_reservada >= {0};";

        await _db.Database.ExecuteSqlRawAsync(sql, cantidad, idLote, idUbicacion, ct);
    }

    public async Task<bool> DescontarAsync(int idLote, int idUbicacion, int cantidad, CancellationToken ct)
    {
        var sql = @"
            UPDATE existencia
            SET cantidad = cantidad - {0}
            WHERE id_lote = {1} AND id_ubicacion = {2}
            RETURNING cantidad;";

        var resultado = await _db.Database
            .SqlQueryRaw<int>(sql, cantidad, idLote, idUbicacion)
            .FirstOrDefaultAsync(ct);

        return resultado >= 0; // Permite negativo (genera alerta en Kardex)
    }

    public async Task SumarAsync(int idLote, int idUbicacion, int cantidad, CancellationToken ct)
    {
        var sql = @"
            INSERT INTO existencia (id_lote, id_ubicacion, cantidad, cantidad_reservada)
            VALUES ({0}, {1}, {2}, 0)
            ON CONFLICT (id_lote, id_ubicacion) DO UPDATE
            SET cantidad = existencia.cantidad + {2};";

        await _db.Database.ExecuteSqlRawAsync(sql, idLote, idUbicacion, cantidad, ct);
    }
}