using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Inventario.Services;

public sealed class KardexService : IKardexService
{
    private readonly ICentralDbContext _db;

    public KardexService(ICentralDbContext db)
    {
        _db = db;
    }

    public async Task MoverAsync(
        int idLote,
        int idUbicacion,
        int cantidad,
        TipoMovimientoInventario tipo,
        int idUsuario,
        string? motivo,
        string? documentoOrigen,
        long? idDocumento,
        CancellationToken ct = default)
    {
        if (cantidad == 0)
            throw new ArgumentException("La cantidad no puede ser cero");

        var signo = tipo switch
        {
            TipoMovimientoInventario.Recepcion => 1,
            TipoMovimientoInventario.Venta => -1,
            TipoMovimientoInventario.Devolucion => 1,
            TipoMovimientoInventario.TrasladoSalida => -1,
            TipoMovimientoInventario.TrasladoEntrada => 1,
            TipoMovimientoInventario.Ajuste => cantidad > 0 ? 1 : -1,
            TipoMovimientoInventario.Merma => -1,
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };

        var cantidadConSigno = cantidad * signo;

        // Actualización atómica de existencia
        var sqlExistencia = @"
            INSERT INTO existencia (id_lote, id_ubicacion, cantidad, cantidad_reservada)
            VALUES ({0}, {1}, {2}, 0)
            ON CONFLICT (id_lote, id_ubicacion) DO UPDATE
            SET cantidad = existencia.cantidad + {2}
            RETURNING cantidad;";

        var nuevaCantidad = await _db.Database
            .SqlQueryRaw<int>(sqlExistencia, idLote, idUbicacion, cantidadConSigno)
            .FirstAsync(ct);

        if (nuevaCantidad < 0)
        {
            // Generar alerta DISCREPANCIA
            await GenerarAlertaDiscrepanciaAsync(idLote, idUbicacion, nuevaCantidad, ct);
        }

        // Registrar movimiento
        var movimiento = new MovimientoInventario
        {
            IdLote = idLote,
            IdUbicacion = idUbicacion,
            IdUsuario = idUsuario,
            FechaHora = DateTimeOffset.UtcNow,
            Tipo = tipo,
            Cantidad = cantidadConSigno,
            Motivo = motivo,
            DocumentoOrigen = documentoOrigen,
            IdDocumento = idDocumento
        };

        _db.MovimientosInventario.Add(movimiento);
        await _db.SaveChangesAsync(ct);
    }

    private async Task GenerarAlertaDiscrepanciaAsync(int idLote, int idUbicacion, int cantidadNegativa, CancellationToken ct)
    {
        var lote = await _db.Lotes.AsNoTracking().FirstAsync(l => l.IdLote == idLote, ct);
        var producto = await _db.Productos.AsNoTracking().FirstAsync(p => p.IdProducto == lote.IdProducto, ct);

        var alerta = new AlertaInventario
        {
            IdProducto = producto.IdProducto,
            IdUbicacion = idUbicacion,
            IdLote = idLote,
            Tipo = TipoAlertaInventario.Discrepancia,
            FechaGeneracion = DateTimeOffset.UtcNow,
            Mensaje = $"Existencia negativa ({cantidadNegativa}) detectada al sincronizar venta offline. Producto: {producto.NombreComercial}, Lote: {lote.NumeroLote}",
            Atendida = false
        };

        _db.AlertasInventario.Add(alerta);
        await _db.SaveChangesAsync(ct);
    }
}