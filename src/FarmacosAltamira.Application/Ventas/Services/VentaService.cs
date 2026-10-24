using FarmacosAltamira.Application.Caja.Dto;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Common.Dtos;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;

namespace FarmacosAltamira.Application.Ventas.Services;

public sealed class VentaService
{
    private readonly ICentralDbContext _db;
    private readonly IAuditoria _auditoria;
    private readonly IKardexService _kardex;

    public VentaService(ICentralDbContext db, IAuditoria auditoria, IKardexService kardex)
    {
        _db = db;
        _auditoria = auditoria;
        _kardex = kardex;
    }

    public async Task<VentaResultadoDto> RegistrarAsync(VentaCreateDto dto, int idUsuario, CancellationToken ct)
    {
        // Verificar idempotencia por uuid_operacion
        var ventaExistente = await _db.Ventas
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.UuidOperacion == dto.UuidOperacion, ct);

        if (ventaExistente != null)
        {
            return new VentaResultadoDto("YA_RECIBIDA", ventaExistente.IdVenta);
        }

        // Validar corte
        var corte = await _db.CortesCaja
            .Include(c => c.Caja)
            .FirstOrDefaultAsync(c => c.IdCorte == dto.IdCorte && c.Estado == EstadoCorte.Abierto, ct);

        if (corte == null)
            throw new InvalidOperationException("El corte no existe o no está abierto");

        // Validar que la caja pertenece a la ubicación del usuario
        // Nota: El objeto User debe ser inyectado o pasado mediante un IUserContext
        // Por ahora, dejamos la validación pendiente de la implementación de IUserContext
        // var idUbicacionUsuario = _userContext.UbicacionId;
        // if (corte.Caja.IdUbicacion != idUbicacionUsuario && idUbicacionUsuario != 0)
        //     throw new UnauthorizedAccessException("La caja no corresponde a la ubicación del usuario");

        // Recalcular totales
        var subtotalCalculado = dto.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad - d.Descuento);
        var totalCalculado = subtotalCalculado - dto.Descuento + dto.Iva;
        var sumaPagos = dto.Pagos.Sum(p => p.Monto);
        var cambio = dto.Pagos.Sum(p => p.Cambio);

        if (Math.Abs(subtotalCalculado - dto.Subtotal) > 0.01m ||
            Math.Abs(totalCalculado - dto.Total) > 0.01m ||
            Math.Abs(sumaPagos - dto.Total - cambio) > 0.01m)
        {
            throw new InvalidOperationException("Los totales no cuadran");
        }

        // Preparar JSON para sp_registrar_venta
        var payload = new
        {
            uuid_operacion = dto.UuidOperacion,
            folio = dto.Folio,
            id_corte = dto.IdCorte,
            id_usuario = idUsuario,
            fecha_hora = dto.FechaHora,
            subtotal = dto.Subtotal,
            descuento = dto.Descuento,
            iva = dto.Iva,
            total = dto.Total,
            requiere_factura = dto.RequiereFactura,
            detalles = dto.Detalles.Select(d => new
            {
                id_producto = d.IdProducto,
                id_lote = d.IdLote,
                id_promocion = d.IdPromocion,
                cantidad = d.Cantidad,
                precio_unitario = d.PrecioUnitario,
                descuento = d.Descuento,
                importe = d.Importe
            }),
            pagos = dto.Pagos.Select(p => new
            {
                forma_pago = p.FormaPago,
                monto = p.Monto,
                referencia = p.Referencia,
                cambio = p.Cambio
            }),
            receta = dto.Receta != null ? new
            {
                folio_receta = dto.Receta.FolioReceta,
                tipo_receta = dto.Receta.TipoReceta,
                fecha_expedicion = dto.Receta.FechaExpedicion,
                nombre_medico = dto.Receta.NombreMedico,
                cedula_medico = dto.Receta.CedulaMedico,
                institucion = dto.Receta.Institucion,
                nombre_paciente = dto.Receta.NombrePaciente
            } : null
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        // Ejecutar procedimiento almacenado
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var resultado = await _db.Database
                .SqlQueryRaw<VentaSpResultado>($@"SELECT resultado, id_venta FROM sp_registrar_venta({json}::jsonb)")
                .FirstAsync(ct);

            // Registrar bitácora en la misma transacción
            await _auditoria.RegistrarAsync(idUsuario, corte.Caja.IdUbicacion, "POS", "REGISTRAR_VENTA", "venta", resultado.IdVenta.ToString(), $"Venta {dto.Folio} registrada", cancellationToken: ct);

            await tx.CommitAsync(ct);

            return new VentaResultadoDto(resultado.Resultado, resultado.IdVenta);
        }
        catch (PostgresException ex) when (ex.SqlState == "P0001")
        {
            await tx.RollbackAsync(ct);
            throw new InvalidOperationException("VENTA_CONTROLADO_SIN_REGISTRO: " + ex.MessageText);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private sealed record VentaSpResultado(string Resultado, int IdVenta);

    public async Task<PaginatedResponse<VentaDto>> ListarAsync(int? idCorte, DateTimeOffset? desde, DateTimeOffset? hasta, int pagina, int tamano, CancellationToken ct)
    {
        var query = _db.Ventas
            .AsNoTracking()
            .Include(v => v.Usuario)
            .Include(v => v.Corte)
                .ThenInclude(c => c.Caja)
            .AsQueryable();

        if (idCorte.HasValue)
            query = query.Where(v => v.IdCorte == idCorte.Value);

        if (desde.HasValue)
            query = query.Where(v => v.FechaHora >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(v => v.FechaHora <= hasta.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(v => v.FechaHora)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(v => new VentaDto(
                v.IdVenta,
                v.Folio,
                v.UuidOperacion,
                v.IdCorte,
                v.Corte.Caja.Numero.ToString(),
                v.IdUsuario,
                v.Usuario.NombreUsuario,
                v.FechaHora,
                v.Subtotal,
                v.Descuento,
                v.Iva,
                v.Total,
                v.Estado.ToString(),
                v.RequiereFactura,
                v.Sincronizada))
            .ToListAsync(ct);

        return new PaginatedResponse<VentaDto>(items, total, pagina, tamano);
    }

    public async Task<VentaDto?> ObtenerPorIdAsync(int id, CancellationToken ct)
    {
        return await _db.Ventas
            .AsNoTracking()
            .Include(v => v.Usuario)
            .Include(v => v.Corte)
                .ThenInclude(c => c.Caja)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Lote)
            .Include(v => v.Pagos)
            .Where(v => v.IdVenta == id)
            .Select(v => new VentaDto(
                v.IdVenta,
                v.Folio,
                v.UuidOperacion,
                v.IdCorte,
                v.Corte.Caja.Numero.ToString(),
                v.IdUsuario,
                v.Usuario.NombreUsuario,
                v.FechaHora,
                v.Subtotal,
                v.Descuento,
                v.Iva,
                v.Total,
                v.Estado.ToString(),
                v.RequiereFactura,
                v.Sincronizada))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<DevolucionDto> RegistrarDevolucionAsync(DevolucionCreateDto dto, int idUsuario, CancellationToken ct)
    {
        var venta = await _db.Ventas
            .Include(v => v.Detalles)
            .Include(v => v.Corte)
                .ThenInclude(c => c.Caja)
            .FirstOrDefaultAsync(v => v.IdVenta == dto.IdVenta, ct);

        if (venta == null) throw new KeyNotFoundException("Venta no encontrada");
        if (venta.Estado == EstadoVenta.Cancelada) throw new InvalidOperationException("No se puede devolver una venta cancelada");

        // Verificar autorización para devolución - se valida en el controller con policy

        var devolucion = new Devolucion
        {
            IdVenta = dto.IdVenta,
            IdUsuario = idUsuario,
            Folio = $"DEV-{venta.Folio}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            Fecha = DateTimeOffset.UtcNow,
            Motivo = dto.Motivo,
            MontoReembolsado = dto.MontoReembolsado,
            FormaReembolso = dto.FormaReembolso
        };

        _db.Devoluciones.Add(devolucion);

        foreach (var detalleDto in dto.Detalles)
        {
            var detalleVenta = venta.Detalles.FirstOrDefault(d => d.IdDetalle == detalleDto.IdDetalleVenta);
            if (detalleVenta == null) throw new KeyNotFoundException($"Detalle {detalleDto.IdDetalleVenta} no encontrado en la venta");

            var detalleDevolucion = new DetalleDevolucion
            {
                IdDevolucion = devolucion.IdDevolucion,
                IdDetalle = detalleDto.IdDetalleVenta,
                Cantidad = detalleDto.Cantidad,
                ReingresaInventario = detalleDto.ReingresaInventario
            };
            _db.DetallesDevolucion.Add(detalleDevolucion);

            if (detalleDto.ReingresaInventario)
            {
                await _kardex.MoverAsync(
                    detalleVenta.IdLote,
                    venta.Corte.Caja.IdUbicacion,
                    detalleDto.Cantidad,
                    TipoMovimientoInventario.Devolucion,
                    idUsuario,
                    $"Devolución venta {venta.Folio}",
                    "DEVOLUCION",
                    devolucion.IdDevolucion,
                    ct);
            }
        }

        venta.Estado = EstadoVenta.ConDevolucion;
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuario, venta.Corte.Caja.IdUbicacion, "POS", "DEVOLUCION", "devolucion", devolucion.IdDevolucion.ToString(), $"Devolución registrada: {dto.Detalles.Count} líneas", cancellationToken: ct);

        return new DevolucionDto(
            devolucion.IdDevolucion,
            devolucion.Folio,
            devolucion.Fecha,
            devolucion.Motivo,
            devolucion.MontoReembolsado,
            devolucion.FormaReembolso);
    }

    public async Task<CancelacionDto> CancelarAsync(CancelacionCreateDto dto, int idUsuario, CancellationToken ct)
    {
        var venta = await _db.Ventas
            .Include(v => v.Corte)
                .ThenInclude(c => c.Caja)
            .FirstOrDefaultAsync(v => v.IdVenta == dto.IdVenta, ct);

        if (venta == null) throw new KeyNotFoundException("Venta no encontrada");
        if (venta.Estado == EstadoVenta.Cancelada) throw new InvalidOperationException("La venta ya está cancelada");

        // Verificar autorización de encargado - se valida en el controller con policy

        // Si tiene factura, verificar que se puede cancelar
        if (venta.Comprobante != null && venta.Comprobante.Estatus == EstatusComprobanteFiscal.Timbrado)
        {
            throw new InvalidOperationException("No se puede cancelar una venta facturada sin cancelar primero el CFDI");
        }

        var cancelacion = new CancelacionVenta
        {
            IdVenta = dto.IdVenta,
            IdUsuarioSolicita = idUsuario,
            IdUsuarioAutoriza = idUsuario,
            FechaHora = DateTimeOffset.UtcNow,
            Motivo = dto.Motivo,
            CfdiCancelado = false
        };

        _db.CancelacionesVenta.Add(cancelacion);

        // Reingresar inventario
        foreach (var detalle in venta.Detalles)
        {
            await _kardex.MoverAsync(
                detalle.IdLote,
                venta.Corte.Caja.IdUbicacion,
                detalle.Cantidad,
                TipoMovimientoInventario.Devolucion,
                idUsuario,
                $"Cancelación venta {venta.Folio}: {dto.Motivo}",
                "CANCELACION_VENTA",
                cancelacion.IdCancelacion,
                ct);
        }

        venta.Estado = EstadoVenta.Cancelada;
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuario, venta.Corte.Caja.IdUbicacion, "POS", "CANCELAR_VENTA", "cancelacion_venta", cancelacion.IdCancelacion.ToString(), $"Venta cancelada: {dto.Motivo}", cancellationToken: ct);

        return new CancelacionDto(cancelacion.IdCancelacion, cancelacion.FechaHora, cancelacion.Motivo);
    }
}

public sealed record VentaDto(
    int IdVenta,
    string Folio,
    Guid UuidOperacion,
    int IdCorte,
    string CajaNumero,
    int IdUsuario,
    string UsuarioNombre,
    DateTimeOffset FechaHora,
    decimal Subtotal,
    decimal Descuento,
    decimal Iva,
    decimal Total,
    string Estado,
    bool RequiereFactura,
    bool Sincronizada);

public sealed record DevolucionDto(
    int IdDevolucion,
    string Folio,
    DateTimeOffset Fecha,
    string Motivo,
    decimal MontoReembolsado,
    string? FormaReembolso);

public sealed record CancelacionDto(
    int IdCancelacion,
    DateTimeOffset FechaHora,
    string Motivo);