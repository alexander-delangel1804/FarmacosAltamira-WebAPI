using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Common.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Seguridad.Services;

public sealed class RolService
{
    private readonly ICentralDbContext _db;
    private readonly IAuditoria _auditoria;

    public RolService(ICentralDbContext db, IAuditoria auditoria)
    {
        _db = db;
        _auditoria = auditoria;
    }

    public async Task<PaginatedResponse<RolDto>> ListarAsync(int pagina, int tamano, CancellationToken ct)
    {
        var query = _db.Roles
            .AsNoTracking()
            .Include(r => r.Permisos)
            .OrderBy(r => r.Nombre);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(r => new RolDto(
                r.IdRol,
                r.Nombre,
                r.Descripcion,
                r.Permisos.Select(p => new PermisoDto(p.IdPermiso, p.Modulo, p.Accion)).ToList()))
            .ToListAsync(ct);

        return new PaginatedResponse<RolDto>(items, total, pagina, tamano);
    }

    public async Task<RolDto?> ObtenerPorIdAsync(int id, CancellationToken ct)
    {
        return await _db.Roles
            .AsNoTracking()
            .Include(r => r.Permisos)
            .Where(r => r.IdRol == id)
            .Select(r => new RolDto(
                r.IdRol,
                r.Nombre,
                r.Descripcion,
                r.Permisos.Select(p => new PermisoDto(p.IdPermiso, p.Modulo, p.Accion)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<RolDto> CrearAsync(RolCreateDto dto, int idUsuarioAudit, CancellationToken ct)
    {
        if (await _db.Roles.AnyAsync(r => r.Nombre == dto.Nombre, ct))
            throw new InvalidOperationException("El nombre del rol ya existe");

        var permisos = await _db.Permisos.Where(p => dto.PermisosIds.Contains(p.IdPermiso)).ToListAsync(ct);
        if (permisos.Count != dto.PermisosIds.Count)
            throw new KeyNotFoundException("Uno o más permisos no existen");

        var rol = new Rol
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Permisos = permisos
        };

        _db.Roles.Add(rol);
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, null, "SEGURIDAD", "CREAR_ROL", "rol", rol.IdRol.ToString(), $"Rol creado: {rol.Nombre}", cancellationToken: ct);

        return await ObtenerPorIdAsync(rol.IdRol, ct) ?? throw new InvalidOperationException("Error al crear rol");
    }

    public async Task<RolDto> ActualizarAsync(int id, RolUpdateDto dto, int idUsuarioAudit, CancellationToken ct)
    {
        var rol = await _db.Roles
            .Include(r => r.Permisos)
            .FirstOrDefaultAsync(r => r.IdRol == id, ct);

        if (rol == null) throw new KeyNotFoundException("Rol no encontrado");

        var permisos = await _db.Permisos.Where(p => dto.PermisosIds.Contains(p.IdPermiso)).ToListAsync(ct);
        if (permisos.Count != dto.PermisosIds.Count)
            throw new KeyNotFoundException("Uno o más permisos no existen");

        rol.Descripcion = dto.Descripcion;
        rol.Permisos = permisos;

        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, null, "SEGURIDAD", "ACTUALIZAR_ROL", "rol", id.ToString(), $"Rol actualizado: {rol.Nombre}", cancellationToken: ct);

        return await ObtenerPorIdAsync(id, ct) ?? throw new InvalidOperationException("Error al actualizar rol");
    }

    public async Task<PaginatedResponse<PermisoDto>> ListarPermisosAsync(int pagina, int tamano, CancellationToken ct)
    {
        var query = _db.Permisos.AsNoTracking().OrderBy(p => p.Modulo).ThenBy(p => p.Accion);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(p => new PermisoDto(p.IdPermiso, p.Modulo, p.Accion))
            .ToListAsync(ct);

        return new PaginatedResponse<PermisoDto>(items, total, pagina, tamano);
    }
}