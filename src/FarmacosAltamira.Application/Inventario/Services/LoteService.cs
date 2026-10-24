using FarmacosAltamira.Application.Inventario.Dto;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Common.Dtos;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Inventario.Services;

public sealed class LoteService
{
    private readonly ICentralDbContext _db;
    private readonly IAuditoria _auditoria;

    public LoteService(ICentralDbContext db, IAuditoria auditoria)
    {
        _db = db;
        _auditoria = auditoria;
    }

    public async Task<PaginatedResponse<LoteDto>> ListarAsync(int? idProducto, int pagina, int tamano, CancellationToken ct)
    {
        var query = _db.Lotes
            .AsNoTracking()
            .Include(l => l.Producto)
            .AsQueryable();

        if (idProducto.HasValue)
            query = query.Where(l => l.IdProducto == idProducto.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(l => l.Producto.NombreComercial)
            .ThenBy(l => l.FechaCaducidad)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(l => new LoteDto(
                l.IdLote,
                l.IdProducto,
                l.Producto.NombreComercial,
                l.Producto.CodigoBarras,
                l.NumeroLote,
                l.FechaCaducidad,
                l.FechaIngreso,
                l.Estado.ToString()))
            .ToListAsync(ct);

        return new PaginatedResponse<LoteDto>(items, total, pagina, tamano);
    }

    public async Task<PaginatedResponse<ExistenciaDto>> ConsultarExistenciasAsync(int? idProducto, int? idUbicacion, int pagina, int tamano, CancellationToken ct)
    {
        var query = _db.Existencias
            .AsNoTracking()
            .Include(e => e.Lote)
                .ThenInclude(l => l.Producto)
            .Include(e => e.Ubicacion)
            .Where(e => e.Cantidad > 0);

        if (idProducto.HasValue)
            query = query.Where(e => e.Lote.IdProducto == idProducto.Value);

        if (idUbicacion.HasValue)
            query = query.Where(e => e.IdUbicacion == idUbicacion.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.Lote.Producto.NombreComercial)
            .ThenBy(e => e.Lote.FechaCaducidad)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(e => new ExistenciaDto(
                e.Lote.IdProducto,
                e.Lote.Producto.NombreComercial,
                e.Lote.Producto.CodigoBarras,
                e.IdLote,
                e.Lote.NumeroLote,
                e.Lote.FechaCaducidad,
                e.IdUbicacion,
                e.Ubicacion.Nombre,
                e.Cantidad,
                e.CantidadReservada,
                e.Disponible))
            .ToListAsync(ct);

        return new PaginatedResponse<ExistenciaDto>(items, total, pagina, tamano);
    }

    public async Task<PaginatedResponse<MovimientoDto>> ConsultarMovimientosAsync(int? idLote, int? idUbicacion, DateTimeOffset? desde, DateTimeOffset? hasta, int pagina, int tamano, CancellationToken ct)
    {
        var query = _db.MovimientosInventario
            .AsNoTracking()
            .Include(m => m.Lote)
                .ThenInclude(l => l.Producto)
            .Include(m => m.Ubicacion)
            .Include(m => m.Usuario)
            .AsQueryable();

        if (idLote.HasValue)
            query = query.Where(m => m.IdLote == idLote.Value);

        if (idUbicacion.HasValue)
            query = query.Where(m => m.IdUbicacion == idUbicacion.Value);

        if (desde.HasValue)
            query = query.Where(m => m.FechaHora >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(m => m.FechaHora <= hasta.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.FechaHora)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(m => new MovimientoDto(
                m.IdMovimiento,
                m.IdLote,
                m.Lote.Producto.NombreComercial,
                m.Lote.NumeroLote,
                m.IdUbicacion,
                m.Ubicacion.Nombre,
                m.Usuario.NombreUsuario,
                m.FechaHora,
                m.Tipo.ToString(),
                m.Cantidad,
                m.Motivo,
                m.DocumentoOrigen,
                m.IdDocumento))
            .ToListAsync(ct);

        return new PaginatedResponse<MovimientoDto>(items, total, pagina, tamano);
    }

    public async Task<RecepcionDto> RegistrarRecepcionAsync(RecepcionCreateDto dto, int idUsuario, CancellationToken ct)
    {
        var ubicacion = await _db.Ubicaciones.FirstOrDefaultAsync(u => u.IdUbicacion == dto.IdUbicacion && u.Tipo == TipoUbicacion.Bodega, ct);
        if (ubicacion == null) throw new InvalidOperationException("La ubicación debe ser una bodega");

        var recepcion = new RecepcionMercancia
        {
            IdUbicacion = dto.IdUbicacion,
            IdUsuario = idUsuario,
            FechaHora = DateTimeOffset.UtcNow,
            DocumentoReferencia = dto.DocumentoReferencia,
            Observaciones = dto.Observaciones
        };

        _db.RecepcionesMercancia.Add(recepcion);
        await _db.SaveChangesAsync(ct);

        foreach (var detalle in dto.Detalles)
        {
            var producto = await _db.Productos.FirstOrDefaultAsync(p => p.IdProducto == detalle.IdProducto && p.Activo, ct);
            if (producto == null) throw new KeyNotFoundException($"Producto {detalle.IdProducto} no encontrado");

            var lote = await _db.Lotes
                .FirstOrDefaultAsync(l => l.IdProducto == detalle.IdProducto && l.NumeroLote == detalle.NumeroLote, ct);

            if (lote == null)
            {
                lote = new Lote
                {
                    IdProducto = detalle.IdProducto,
                    NumeroLote = detalle.NumeroLote,
                    FechaCaducidad = detalle.FechaCaducidad,
                    FechaIngreso = DateOnly.FromDateTime(DateTime.UtcNow),
                    Estado = EstadoLote.Vigente
                };
                _db.Lotes.Add(lote);
                await _db.SaveChangesAsync(ct);
            }

            var detalleRecepcion = new DetalleRecepcion
            {
                IdRecepcion = recepcion.IdRecepcion,
                IdLote = lote.IdLote,
                CantidadRecibida = detalle.Cantidad
            };
            _db.DetallesRecepcion.Add(detalleRecepcion);

            // Actualizar existencia y kárdex
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO existencia (id_lote, id_ubicacion, cantidad, cantidad_reservada)
                VALUES ({lote.IdLote}, {dto.IdUbicacion}, {detalle.Cantidad}, 0)
                ON CONFLICT (id_lote, id_ubicacion) DO UPDATE
                SET cantidad = existencia.cantidad + {detalle.Cantidad};", ct);

            var movimiento = new MovimientoInventario
            {
                IdLote = lote.IdLote,
                IdUbicacion = dto.IdUbicacion,
                IdUsuario = idUsuario,
                FechaHora = DateTimeOffset.UtcNow,
                Tipo = TipoMovimientoInventario.Recepcion,
                Cantidad = detalle.Cantidad,
                DocumentoOrigen = "RECEPCION",
                IdDocumento = recepcion.IdRecepcion
            };
            _db.MovimientosInventario.Add(movimiento);
        }

        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuario, dto.IdUbicacion, "BODEGA", "RECEPCION_MERCANCIA", "recepcion_mercancia", recepcion.IdRecepcion.ToString(), $"Recepción registrada con {dto.Detalles.Count} líneas", cancellationToken: ct);

        return await ObtenerRecepcionAsync(recepcion.IdRecepcion, ct) ?? throw new InvalidOperationException("Error al obtener recepción");
    }

    public async Task<AjusteCreateDto> RegistrarAjusteAsync(AjusteCreateDto dto, int idUsuario, CancellationToken ct)
    {
        var lote = await _db.Lotes.AsNoTracking().FirstOrDefaultAsync(l => l.IdLote == dto.IdLote, ct);
        if (lote == null) throw new KeyNotFoundException("Lote no encontrado");

        var ubicacion = await _db.Ubicaciones.AsNoTracking().FirstOrDefaultAsync(u => u.IdUbicacion == dto.IdUbicacion, ct);
        if (ubicacion == null) throw new KeyNotFoundException("Ubicación no encontrada");

        var tipo = dto.Cantidad >= 0 ? TipoMovimientoInventario.Ajuste : TipoMovimientoInventario.Merma;

        await _db.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO existencia (id_lote, id_ubicacion, cantidad, cantidad_reservada)
            VALUES ({dto.IdLote}, {dto.IdUbicacion}, {dto.Cantidad}, 0)
            ON CONFLICT (id_lote, id_ubicacion) DO UPDATE
            SET cantidad = existencia.cantidad + {dto.Cantidad};", ct);

        var movimiento = new MovimientoInventario
        {
            IdLote = dto.IdLote,
            IdUbicacion = dto.IdUbicacion,
            IdUsuario = idUsuario,
            FechaHora = DateTimeOffset.UtcNow,
            Tipo = tipo,
            Cantidad = dto.Cantidad,
            Motivo = dto.Motivo,
            DocumentoOrigen = "AJUSTE"
        };
        _db.MovimientosInventario.Add(movimiento);
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuario, dto.IdUbicacion, "BODEGA", tipo == TipoMovimientoInventario.Ajuste ? "AJUSTAR_INVENTARIO" : "REGISTRAR_MERMA", "movimiento_inventario", movimiento.IdMovimiento.ToString(), $"Ajuste: {dto.Cantidad} unidades. Motivo: {dto.Motivo}", cancellationToken: ct);

        return dto;
    }

    public async Task<PaginatedResponse<AlertaDto>> ListarAlertasAsync(int? idUbicacion, bool soloNoAtendidas, int pagina, int tamano, CancellationToken ct)
    {
        var query = _db.AlertasInventario
            .AsNoTracking()
            .Include(a => a.Producto)
            .Include(a => a.Ubicacion)
            .Include(a => a.Lote)
            .AsQueryable();

        if (idUbicacion.HasValue)
            query = query.Where(a => a.IdUbicacion == idUbicacion.Value);

        if (soloNoAtendidas)
            query = query.Where(a => !a.Atendida);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.FechaGeneracion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(a => new AlertaDto(
                a.IdAlerta,
                a.IdProducto,
                a.Producto.NombreComercial,
                a.IdUbicacion,
                a.Ubicacion.Nombre,
                a.IdLote,
                a.Lote != null ? a.Lote.NumeroLote : null,
                a.Tipo.ToString(),
                a.FechaGeneracion,
                a.Mensaje,
                a.Atendida))
            .ToListAsync(ct);

        return new PaginatedResponse<AlertaDto>(items, total, pagina, tamano);
    }

    public async Task MarcarAlertaAtendidaAsync(int idAlerta, int idUsuario, CancellationToken ct)
    {
        var alerta = await _db.AlertasInventario.FirstOrDefaultAsync(a => a.IdAlerta == idAlerta, ct);
        if (alerta == null) throw new KeyNotFoundException("Alerta no encontrada");

        alerta.Atendida = true;
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuario, alerta.IdUbicacion, "BODEGA", "ATENDER_ALERTA", "alerta_inventario", idAlerta.ToString(), "Alerta marcada como atendida", cancellationToken: ct);
    }

    public async Task<RecepcionDto?> ObtenerRecepcionPorIdAsync(int id, CancellationToken ct)
    {
        return await ObtenerRecepcionAsync(id, ct);
    }

    private async Task<RecepcionDto?> ObtenerRecepcionAsync(int id, CancellationToken ct)
    {
        return await _db.RecepcionesMercancia
            .AsNoTracking()
            .Include(r => r.Ubicacion)
            .Include(r => r.Usuario)
            .Include(r => r.Detalles)
                .ThenInclude(d => d.Lote)
                    .ThenInclude(l => l.Producto)
            .Where(r => r.IdRecepcion == id)
            .Select(r => new RecepcionDto(
                r.IdRecepcion,
                r.IdUbicacion,
                r.Ubicacion.Nombre,
                r.IdUsuario,
                r.Usuario.NombreUsuario,
                r.FechaHora,
                r.DocumentoReferencia,
                r.Observaciones,
                r.Detalles.Select(d => new DetalleRecepcionDto(
                    d.IdLote,
                    d.Lote.Producto.NombreComercial,
                    d.Lote.NumeroLote,
                    d.CantidadRecibida)).ToList()))
            .FirstOrDefaultAsync(ct);
    }
}