using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Common.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Seguridad.Services;

public sealed class ParametroService
{
    private readonly ICentralDbContext _db;
    private readonly IAuditoria _auditoria;

    public ParametroService(ICentralDbContext db, IAuditoria auditoria)
    {
        _db = db;
        _auditoria = auditoria;
    }

    public async Task<PaginatedResponse<ParametroDto>> ListarAsync(int pagina, int tamano, CancellationToken ct)
    {
        var query = _db.Parametros.AsNoTracking().OrderBy(p => p.Clave);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(p => new ParametroDto(p.Clave, p.Valor, p.Descripcion, p.FechaModificacion))
            .ToListAsync(ct);

        return new PaginatedResponse<ParametroDto>(items, total, pagina, tamano);
    }

    public async Task<ParametroDto?> ObtenerAsync(string clave, CancellationToken ct)
    {
        var p = await _db.Parametros.AsNoTracking().FirstOrDefaultAsync(x => x.Clave == clave, ct);
        return p == null ? null : new ParametroDto(p.Clave, p.Valor, p.Descripcion, p.FechaModificacion);
    }

    public async Task ActualizarAsync(string clave, ParametroUpdateDto dto, int idUsuarioAudit, CancellationToken ct)
    {
        var param = await _db.Parametros.FirstOrDefaultAsync(x => x.Clave == clave, ct);
        if (param == null) throw new KeyNotFoundException("Parámetro no encontrado");

        var valorAnterior = param.Valor;
        param.Valor = dto.Valor;
        param.FechaModificacion = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, null, "SEGURIDAD", "CAMBIAR_PARAMETRO", "parametro_sistema", clave, $"Valor cambiado de '{valorAnterior}' a '{dto.Valor}'", cancellationToken: ct);
    }

    public async Task<int> ObtenerEnteroAsync(string clave, int valorPorDefecto, CancellationToken ct)
    {
        var p = await _db.Parametros.AsNoTracking().FirstOrDefaultAsync(x => x.Clave == clave, ct);
        return p != null && int.TryParse(p.Valor, out var v) ? v : valorPorDefecto;
    }
}