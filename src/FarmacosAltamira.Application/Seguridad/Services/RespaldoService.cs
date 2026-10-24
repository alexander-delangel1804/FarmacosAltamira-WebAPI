using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Common.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Seguridad.Services;

public sealed class RespaldoService
{
    private readonly ICentralDbContext _db;
    private readonly IAuditoria _auditoria;

    public RespaldoService(ICentralDbContext db, IAuditoria auditoria)
    {
        _db = db;
        _auditoria = auditoria;
    }

    public async Task<PaginatedResponse<RespaldoDto>> ListarAsync(RespaldoFiltroDto filtro, CancellationToken ct)
    {
        IQueryable<Respaldo> query = _db.Respaldos
            .AsNoTracking()
            .Include(r => r.Usuario);
        
        if (filtro.IdUsuario.HasValue)
            query = query.Where(r => r.IdUsuario == filtro.IdUsuario.Value);
        
        if (!string.IsNullOrEmpty(filtro.Tipo))
            query = query.Where(r => r.Tipo == filtro.Tipo);
        
        if (!string.IsNullOrEmpty(filtro.Estado))
            query = query.Where(r => r.Estado == filtro.Estado);
        
        if (filtro.FechaDesde.HasValue)
            query = query.Where(r => r.FechaHora >= filtro.FechaDesde.Value);
        
        if (filtro.FechaHasta.HasValue)
            query = query.Where(r => r.FechaHora <= filtro.FechaHasta.Value);

        var orderedQuery = query.OrderByDescending(r => r.FechaHora);
        var total = await orderedQuery.CountAsync(ct);
        var items = await orderedQuery
            .Skip(filtro.Offset)
            .Take(filtro.TamanoPagina)
            .Select(r => new RespaldoDto(
                r.IdRespaldo,
                r.IdUsuario,
                r.Usuario != null ? r.Usuario.NombreUsuario : null,
                r.FechaHora,
                r.Tipo,
                r.Ruta,
                r.TamanoMb,
                r.Cifrado,
                r.Estado,
                r.Verificado))
            .ToListAsync(ct);
        
        return new PaginatedResponse<RespaldoDto>(items, total, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<RespaldoDto> RegistrarInicioAsync(int? idUsuario, string tipo, string ruta, CancellationToken ct)
    {
        var respaldo = new Respaldo
        {
            IdUsuario = idUsuario,
            FechaHora = DateTimeOffset.UtcNow,
            Tipo = tipo,
            Ruta = ruta,
            Estado = "EN_CURSO"
        };

        _db.Respaldos.Add(respaldo);
        await _db.SaveChangesAsync(ct);

        if (idUsuario.HasValue)
        {
            await _auditoria.RegistrarAsync(idUsuario.Value, null, "SEGURIDAD", "RESPALDO_INICIADO", "respaldo", respaldo.IdRespaldo.ToString(), $"Respaldo {tipo} iniciado", cancellationToken: ct);
        }

        return new RespaldoDto(
            respaldo.IdRespaldo,
            respaldo.IdUsuario,
            null,
            respaldo.FechaHora,
            respaldo.Tipo,
            respaldo.Ruta,
            respaldo.TamanoMb,
            respaldo.Cifrado,
            respaldo.Estado,
            respaldo.Verificado);
    }

    public async Task FinalizarAsync(int idRespaldo, bool exitoso, decimal? tamanoMb, int? idUsuarioAudit, CancellationToken ct)
    {
        var respaldo = await _db.Respaldos.FirstOrDefaultAsync(r => r.IdRespaldo == idRespaldo, ct);
        if (respaldo == null) throw new KeyNotFoundException("Respaldo no encontrado");

        respaldo.Estado = exitoso ? "EXITOSO" : "FALLIDO";
        respaldo.TamanoMb = tamanoMb;
        await _db.SaveChangesAsync(ct);

        if (idUsuarioAudit.HasValue)
        {
            await _auditoria.RegistrarAsync(idUsuarioAudit.Value, null, "SEGURIDAD", "RESPALDO_FINALIZADO", "respaldo", idRespaldo.ToString(), $"Respaldo {respaldo.Estado}", cancellationToken: ct);
        }
    }

    public async Task MarcarVerificadoAsync(int idRespaldo, int idUsuarioAudit, CancellationToken ct)
    {
        var respaldo = await _db.Respaldos.FirstOrDefaultAsync(r => r.IdRespaldo == idRespaldo, ct);
        if (respaldo == null) throw new KeyNotFoundException("Respaldo no encontrado");

        respaldo.Verificado = true;
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, null, "SEGURIDAD", "RESPALDO_VERIFICADO", "respaldo", idRespaldo.ToString(), "Respaldo verificado mediante prueba de restauración", cancellationToken: ct);
    }
}