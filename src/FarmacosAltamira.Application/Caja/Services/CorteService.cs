using FarmacosAltamira.Application.Caja.Dto;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Caja.Services;

    public sealed class CorteService
    {
        private readonly ICentralDbContext _db;
        private readonly IAuditoria _auditoria;

        public CorteService(ICentralDbContext db, IAuditoria auditoria)
        {
            _db = db;
            _auditoria = auditoria;
        }

    public async Task<CorteDto> AbrirAsync(CorteAperturaDto dto, int idUsuario, CancellationToken ct)
    {
        var caja = await _db.Cajas.FirstOrDefaultAsync(c => c.IdCaja == dto.IdCaja && c.Activa, ct);
        if (caja == null) throw new KeyNotFoundException("Caja no encontrada");

        // Verificar si ya existe un corte abierto para esta caja (idempotencia por uuid_operacion)
        var corteExistente = await _db.CortesCaja
            .FirstOrDefaultAsync(c => c.UuidOperacion == dto.UuidOperacion, ct);

        if (corteExistente != null)
        {
            return await MapToDto(corteExistente, ct);
        }

        var corte = new CorteCaja
        {
            IdCaja = dto.IdCaja,
            IdUsuario = idUsuario,
            FechaApertura = DateTimeOffset.UtcNow,
            FondoInicial = dto.FondoInicial,
            Estado = EstadoCorte.Abierto,
            UuidOperacion = dto.UuidOperacion
        };

        _db.CortesCaja.Add(corte);
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuario, caja.IdUbicacion, "POS", "ABRIR_CORTE", "corte_caja", corte.IdCorte.ToString(), $"Corte abierto con fondo inicial {dto.FondoInicial}", cancellationToken: ct);

        return await MapToDto(corte, ct);
    }

    public async Task<CorteDto> CerrarAsync(int idCorte, CorteCierreDto dto, int idUsuario, CancellationToken ct)
    {
        var corte = await _db.CortesCaja
            .Include(c => c.Caja)
            .Include(c => c.Ventas)
                .ThenInclude(v => v.Pagos)
            .Include(c => c.PagosServicio)
            .FirstOrDefaultAsync(c => c.IdCorte == idCorte, ct);

        if (corte == null) throw new KeyNotFoundException("Corte no encontrado");

        if (corte.Estado == EstadoCorte.Cerrado)
        {
            // Idempotente: si ya está cerrado, devolver el mismo resultado
            return await MapToDto(corte, ct);
        }

        // Calcular total del sistema
        var totalVentas = corte.Ventas.Where(v => v.Estado != EstadoVenta.Cancelada).Sum(v => v.Total);
        var totalDevoluciones = corte.Ventas.SelectMany(v => v.Detalles?.Where(d => d != null) ?? Enumerable.Empty<DetalleVenta>()).Sum(d => 0m);
        var totalPagosServicio = corte.PagosServicio.Where(p => p.Estatus == EstatusPagoServicio.Autorizado).Sum(p => p.TotalCobrado);

        var totalSistema = totalVentas - totalDevoluciones + totalPagosServicio;
        var diferencia = dto.TotalContado - totalSistema;

        corte.FechaCierre = DateTimeOffset.UtcNow;
        corte.TotalSistema = totalSistema;
        corte.TotalContado = dto.TotalContado;
        corte.Diferencia = diferencia;
        corte.Estado = EstadoCorte.Cerrado;

        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuario, corte.Caja.IdUbicacion, "POS", "CERRAR_CORTE", "corte_caja", idCorte.ToString(), $"Corte cerrado. Total sistema: {totalSistema}, Contado: {dto.TotalContado}, Diferencia: {diferencia}", cancellationToken: ct);

        return await MapToDto(corte, ct);
    }

    private async Task<CorteDto> MapToDto(CorteCaja corte, CancellationToken ct)
    {
        var caja = await _db.Cajas.AsNoTracking().FirstAsync(c => c.IdCaja == corte.IdCaja, ct);
        var usuario = await _db.Usuarios.AsNoTracking().FirstAsync(u => u.IdUsuario == corte.IdUsuario, ct);

        return new CorteDto(
            corte.IdCorte,
            corte.IdCaja,
            caja.Numero.ToString(),
            corte.IdUsuario,
            usuario.NombreUsuario,
            corte.FechaApertura,
            corte.FechaCierre,
            corte.FondoInicial,
            corte.TotalSistema,
            corte.TotalContado,
            corte.Diferencia,
            corte.Estado.ToString(),
            corte.UuidOperacion);
    }
}