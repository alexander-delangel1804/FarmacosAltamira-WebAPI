using FarmacosAltamira.Application.Traslados.Dto;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Common.Dtos;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Traslados.Services;

public sealed class TrasladoService
{
    private readonly ICentralDbContext _db;
    private readonly IAuditoria _auditoria;
    private readonly IKardexService _kardex;
    private readonly IExistenciaService _existencia;

    public TrasladoService(ICentralDbContext db, IAuditoria auditoria, IKardexService kardex, IExistenciaService existencia)
    {
        _db = db;
        _auditoria = auditoria;
        _kardex = kardex;
        _existencia = existencia;
    }

    public async Task<TrasladoDto> SolicitarAsync(TrasladoCreateDto dto, int idUsuario, CancellationToken ct)
    {
        if (dto.IdOrigen == dto.IdDestino)
            throw new InvalidOperationException("El origen y destino no pueden ser la misma ubicación");

        var origen = await _db.Ubicaciones.FirstOrDefaultAsync(u => u.IdUbicacion == dto.IdOrigen && u.Activa, ct);
        var destino = await _db.Ubicaciones.FirstOrDefaultAsync(u => u.IdUbicacion == dto.IdDestino && u.Activa, ct);

        if (origen == null || destino == null)
            throw new KeyNotFoundException("Origen o destino no encontrados");

        // Verificar disponibilidad y reservar
        foreach (var detalle in dto.Detalles)
        {
            var disponible = await _existencia.ObtenerDisponibleAsync(detalle.IdLote, dto.IdOrigen, ct);
            if (disponible < detalle.CantidadEnviada)
                throw new InvalidOperationException($"Existencia insuficiente en lote {detalle.IdLote}. Disponible: {disponible}, Solicitado: {detalle.CantidadEnviada}");

            var reservado = await _existencia.ReservarAsync(detalle.IdLote, dto.IdOrigen, detalle.CantidadEnviada, ct);
            if (!reservado)
                throw new InvalidOperationException($"No se pudo reservar existencia para lote {detalle.IdLote}");
        }

        var folio = $"TRA-{DateTimeOffset.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        var traslado = new Traslado
        {
            Folio = folio,
            IdOrigen = dto.IdOrigen,
            IdDestino = dto.IdDestino,
            IdUsuarioSolicita = idUsuario,
            FechaSolicitud = DateTimeOffset.UtcNow,
            Estado = EstadoTraslado.Solicitado,
            Observaciones = dto.Observaciones
        };

        _db.Traslados.Add(traslado);
        await _db.SaveChangesAsync(ct);

        foreach (var detalle in dto.Detalles)
        {
            _db.DetallesTraslado.Add(new DetalleTraslado
            {
                IdTraslado = traslado.IdTraslado,
                IdLote = detalle.IdLote,
                CantidadEnviada = detalle.CantidadEnviada
            });
        }

        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuario, dto.IdOrigen, "BODEGA", "SOLICITAR_TRASLADO", "traslado", traslado.IdTraslado.ToString(), $"Traslado solicitado: {folio}", cancellationToken: ct);

        return await ObtenerPorIdAsync(traslado.IdTraslado, ct) ?? throw new InvalidOperationException("Error al obtener traslado");
    }

    public async Task<TrasladoDto> DespacharAsync(int idTraslado, int idUsuario, CancellationToken ct)
    {
        var traslado = await _db.Traslados
            .Include(t => t.Detalles)
            .Include(t => t.Origen)
            .Include(t => t.Destino)
            .FirstOrDefaultAsync(t => t.IdTraslado == idTraslado, ct);

        if (traslado == null) throw new KeyNotFoundException("Traslado no encontrado");
        if (traslado.Estado != EstadoTraslado.Solicitado)
            throw new InvalidOperationException("Solo se pueden despachar traslados en estado SOLICITADO");

        // Verificar concurrencia con xmin
        var xminActual = traslado.Xmin;
        var filas = await _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE traslado SET estado = 'EN_TRANSITO', fecha_envio = now(), xmin = xmin
            WHERE id_traslado = {idTraslado} AND xmin = {xminActual};", ct);

        if (filas == 0)
            throw new InvalidOperationException("El traslado fue modificado por otro usuario. Recargue e intente de nuevo.");

        // Liberar reserva y descontar en origen (TRASLADO_SALIDA)
        foreach (var detalle in traslado.Detalles)
        {
            await _existencia.LiberarReservaAsync(detalle.IdLote, traslado.IdOrigen, detalle.CantidadEnviada, ct);
            await _kardex.MoverAsync(
                detalle.IdLote,
                traslado.IdOrigen,
                detalle.CantidadEnviada,
                TipoMovimientoInventario.TrasladoSalida,
                idUsuario,
                $"Despacho traslado {traslado.Folio}",
                "TRASLADO",
                traslado.IdTraslado,
                ct);
        }

        await _auditoria.RegistrarAsync(idUsuario, traslado.IdOrigen, "BODEGA", "DESPACHAR_TRASLADO", "traslado", idTraslado.ToString(), $"Traslado despachado: {traslado.Folio}", cancellationToken: ct);

        return await ObtenerPorIdAsync(idTraslado, ct) ?? throw new InvalidOperationException("Error al obtener traslado");
    }

    public async Task<TrasladoDto> RecibirAsync(int idTraslado, TrasladoRecibirDto dto, int idUsuario, CancellationToken ct)
    {
        var traslado = await _db.Traslados
            .Include(t => t.Detalles)
            .Include(t => t.Origen)
            .Include(t => t.Destino)
            .FirstOrDefaultAsync(t => t.IdTraslado == idTraslado, ct);

        if (traslado == null) throw new KeyNotFoundException("Traslado no encontrado");
        if (traslado.Estado != EstadoTraslado.EnTransito)
            throw new InvalidOperationException("Solo se pueden recibir traslados en estado EN_TRANSITO");

        var xminActual = traslado.Xmin;
        var filas = await _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE traslado SET estado = 'RECIBIDO', fecha_recepcion = now(), xmin = xmin
            WHERE id_traslado = {idTraslado} AND xmin = {xminActual};", ct);

        if (filas == 0)
            throw new InvalidOperationException("El traslado fue modificado por otro usuario. Recargue e intente de nuevo.");

        // Actualizar cantidades recibidas y sumar en destino (TRASLADO_ENTRADA)
        var detalleDict = dto.Detalles.ToDictionary(d => d.IdLote, d => d.CantidadRecibida);

        foreach (var detalle in traslado.Detalles)
        {
            var recibida = detalleDict.GetValueOrDefault(detalle.IdLote, 0);
            detalle.CantidadRecibida = recibida;

            if (recibida > 0)
            {
                await _kardex.MoverAsync(
                    detalle.IdLote,
                    traslado.IdDestino,
                    recibida,
                    TipoMovimientoInventario.TrasladoEntrada,
                    idUsuario,
                    $"Recepción traslado {traslado.Folio}",
                    "TRASLADO",
                    traslado.IdTraslado,
                    ct);
            }

            // Si hay diferencia, generar alerta DISCREPANCIA
            if (recibida != detalle.CantidadEnviada)
            {
                var lote = await _db.Lotes.AsNoTracking().FirstAsync(l => l.IdLote == detalle.IdLote, ct);
                var producto = await _db.Productos.AsNoTracking().FirstAsync(p => p.IdProducto == lote.IdProducto, ct);

                var alerta = new AlertaInventario
                {
                    IdProducto = producto.IdProducto,
                    IdUbicacion = traslado.IdDestino,
                    IdLote = detalle.IdLote,
                    Tipo = TipoAlertaInventario.Discrepancia,
                    FechaGeneracion = DateTimeOffset.UtcNow,
                    Mensaje = $"Diferencia en traslado {traslado.Folio}: Enviados {detalle.CantidadEnviada}, Recibidos {recibida}. Producto: {producto.NombreComercial}, Lote: {lote.NumeroLote}",
                    Atendida = false
                };
                _db.AlertasInventario.Add(alerta);
            }
        }

        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuario, traslado.IdDestino, "BODEGA", "RECIBIR_TRASLADO", "traslado", idTraslado.ToString(), $"Traslado recibido: {traslado.Folio}", cancellationToken: ct);

        return await ObtenerPorIdAsync(idTraslado, ct) ?? throw new InvalidOperationException("Error al obtener traslado");
    }

    public async Task<TrasladoDto> CancelarAsync(int idTraslado, int idUsuario, CancellationToken ct)
    {
        var traslado = await _db.Traslados
            .Include(t => t.Detalles)
            .FirstOrDefaultAsync(t => t.IdTraslado == idTraslado, ct);

        if (traslado == null) throw new KeyNotFoundException("Traslado no encontrado");
        if (traslado.Estado != EstadoTraslado.Solicitado)
            throw new InvalidOperationException("Solo se pueden cancelar traslados en estado SOLICITADO");

        // Liberar reservas
        foreach (var detalle in traslado.Detalles)
        {
            await _existencia.LiberarReservaAsync(detalle.IdLote, traslado.IdOrigen, detalle.CantidadEnviada, ct);
        }

        traslado.Estado = EstadoTraslado.Cancelado;
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuario, traslado.IdOrigen, "BODEGA", "CANCELAR_TRASLADO", "traslado", idTraslado.ToString(), $"Traslado cancelado: {traslado.Folio}", cancellationToken: ct);

        return await ObtenerPorIdAsync(idTraslado, ct) ?? throw new InvalidOperationException("Error al obtener traslado");
    }

    public async Task<PaginatedResponse<TrasladoDto>> ListarAsync(EstadoTraslado? estado, int? idOrigen, int? idDestino, int pagina, int tamano, CancellationToken ct)
    {
        var query = _db.Traslados
            .AsNoTracking()
            .Include(t => t.Origen)
            .Include(t => t.Destino)
            .Include(t => t.UsuarioSolicita)
            .Include(t => t.Detalles)
                .ThenInclude(d => d.Lote)
                    .ThenInclude(l => l.Producto)
            .AsQueryable();

        if (estado.HasValue)
            query = query.Where(t => t.Estado == estado.Value);

        if (idOrigen.HasValue)
            query = query.Where(t => t.IdOrigen == idOrigen.Value);

        if (idDestino.HasValue)
            query = query.Where(t => t.IdDestino == idDestino.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.FechaSolicitud)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(t => MapToDto(t))
            .ToListAsync(ct);

        return new PaginatedResponse<TrasladoDto>(items, total, pagina, tamano);
    }

    public async Task<TrasladoDto?> ObtenerPorIdAsync(int id, CancellationToken ct)
    {
        var t = await _db.Traslados
            .AsNoTracking()
            .Include(t => t.Origen)
            .Include(t => t.Destino)
            .Include(t => t.UsuarioSolicita)
            .Include(t => t.Detalles)
                .ThenInclude(d => d.Lote)
                    .ThenInclude(l => l.Producto)
            .FirstOrDefaultAsync(t => t.IdTraslado == id, ct);

        return t == null ? null : MapToDto(t);
    }

    private static TrasladoDto MapToDto(Traslado t)
    {
        return new TrasladoDto(
            t.IdTraslado,
            t.Folio,
            t.IdOrigen,
            t.Origen.Nombre,
            t.IdDestino,
            t.Destino.Nombre,
            t.IdUsuarioSolicita,
            t.UsuarioSolicita.NombreUsuario,
            t.FechaSolicitud,
            t.FechaEnvio,
            t.FechaRecepcion,
            t.Estado.ToString(),
            t.Observaciones,
            t.Detalles.Select(d => new DetalleTrasladoDto(
                d.IdLote,
                d.Lote.Producto.NombreComercial,
                d.Lote.NumeroLote,
                d.CantidadEnviada,
                d.CantidadRecibida)).ToList());
    }
}