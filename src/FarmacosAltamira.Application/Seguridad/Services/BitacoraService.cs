using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Common.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Seguridad.Services;

public sealed class BitacoraService
{
    private readonly ICentralDbContext _db;

    public BitacoraService(ICentralDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedResponse<BitacoraDto>> ConsultarAsync(BitacoraFiltroDto filtro, CancellationToken ct)
    {
        // Forzar el tipo a IQueryable para evitar conflictos con IOrderedQueryable en las asignaciones posteriores
        IQueryable<BitacoraEvento> query = _db.BitacoraEventos
            .AsNoTracking()
            .Include(b => b.Usuario)
            .Include(b => b.Ubicacion);
        
        if (filtro.IdUsuario.HasValue)
            query = query.Where(b => b.IdUsuario == filtro.IdUsuario.Value);
        
        if (filtro.IdUbicacion.HasValue)
            query = query.Where(b => b.IdUbicacion == filtro.IdUbicacion.Value);
        
        if (!string.IsNullOrEmpty(filtro.Modulo))
            query = query.Where(b => b.Modulo == filtro.Modulo);
        
        if (!string.IsNullOrEmpty(filtro.Accion))
            query = query.Where(b => b.Accion == filtro.Accion);
        
        if (filtro.FechaDesde.HasValue)
            query = query.Where(b => b.FechaHora >= filtro.FechaDesde.Value);
        
        if (filtro.FechaHasta.HasValue)
            query = query.Where(b => b.FechaHora <= filtro.FechaHasta.Value);

        var orderedQuery = query.OrderByDescending(b => b.FechaHora);
        var total = await orderedQuery.CountAsync(ct);
        var items = await orderedQuery
            .Skip(filtro.Offset)
            .Take(filtro.TamanoPagina)
            .Select(b => new BitacoraDto(
                b.IdEvento,
                b.IdUsuario,
                b.Usuario.NombreUsuario,
                b.IdUbicacion,
                b.Ubicacion != null ? b.Ubicacion.Nombre : null,
                b.FechaHora,
                b.Modulo,
                b.Accion,
                b.Entidad,
                b.IdEntidad,
                b.Detalle,
                b.IpOrigen))
            .ToListAsync(ct);
        
        return new PaginatedResponse<BitacoraDto>(items, total, filtro.Pagina, filtro.TamanoPagina);
    }
}