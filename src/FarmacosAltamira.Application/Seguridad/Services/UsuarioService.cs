using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Common.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Seguridad.Services;

public sealed class UsuarioService
{
    private readonly ICentralDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditoria _auditoria;

    public UsuarioService(ICentralDbContext db, IPasswordHasher hasher, IAuditoria auditoria)
    {
        _db = db;
        _hasher = hasher;
        _auditoria = auditoria;
    }

    public async Task<UsuarioDto?> ObtenerPorIdAsync(int id, CancellationToken ct)
    {
        return await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .Include(u => u.Ubicacion)
            .Where(u => u.IdUsuario == id)
            .Select(u => new UsuarioDto(
                u.IdUsuario,
                u.NombreUsuario,
                u.NombreCompleto,
                u.Rol.Nombre,
                u.IdUbicacion,
                u.Ubicacion != null ? u.Ubicacion.Nombre : null,
                u.Activo,
                u.Bloqueado,
                u.IntentosFallidos,
                u.UltimoAcceso,
                u.FechaAlta))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PaginatedResponse<UsuarioDto>> ListarAsync(int pagina, int tamano, CancellationToken ct)
    {
        var query = _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .Include(u => u.Ubicacion)
            .OrderBy(u => u.NombreUsuario);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(u => new UsuarioDto(
                u.IdUsuario,
                u.NombreUsuario,
                u.NombreCompleto,
                u.Rol.Nombre,
                u.IdUbicacion,
                u.Ubicacion != null ? u.Ubicacion.Nombre : null,
                u.Activo,
                u.Bloqueado,
                u.IntentosFallidos,
                u.UltimoAcceso,
                u.FechaAlta))
            .ToListAsync(ct);

        return new PaginatedResponse<UsuarioDto>(items, total, pagina, tamano);
    }

    public async Task<UsuarioDto> CrearAsync(UsuarioCreateDto dto, int idUsuarioAudit, CancellationToken ct)
    {
        if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario, ct))
            throw new InvalidOperationException("El nombre de usuario ya existe");

        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario,
            NombreCompleto = dto.NombreCompleto,
            HashContrasena = _hasher.Hash(dto.Contrasena),
            IdRol = dto.IdRol,
            IdUbicacion = dto.IdUbicacion,
            Activo = true,
            Bloqueado = false,
            IntentosFallidos = 0,
            FechaAlta = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, dto.IdUbicacion, "SEGURIDAD", "CREAR_USUARIO", "usuario", usuario.IdUsuario.ToString(), $"Usuario creado: {usuario.NombreUsuario}", cancellationToken: ct);

        return await ObtenerPorIdAsync(usuario.IdUsuario, ct) ?? throw new InvalidOperationException("Error al crear usuario");
    }

    public async Task<UsuarioDto> ActualizarAsync(int id, UsuarioUpdateDto dto, int idUsuarioAudit, CancellationToken ct)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id, ct);
        if (usuario == null) throw new KeyNotFoundException("Usuario no encontrado");

        usuario.NombreCompleto = dto.NombreCompleto;
        usuario.IdRol = dto.IdRol;
        usuario.IdUbicacion = dto.IdUbicacion;
        usuario.Activo = dto.Activo;

        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, usuario.IdUbicacion, "SEGURIDAD", "ACTUALIZAR_USUARIO", "usuario", id.ToString(), $"Usuario actualizado: {usuario.NombreUsuario}", cancellationToken: ct);

        return await ObtenerPorIdAsync(id, ct) ?? throw new InvalidOperationException("Error al actualizar usuario");
    }

    public async Task BloquearAsync(int id, int idUsuarioAudit, CancellationToken ct)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id, ct);
        if (usuario == null) throw new KeyNotFoundException("Usuario no encontrado");

        usuario.Bloqueado = true;
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, usuario.IdUbicacion, "SEGURIDAD", "BLOQUEAR_CUENTA", "usuario", id.ToString(), $"Usuario bloqueado: {usuario.NombreUsuario}", cancellationToken: ct);
    }

    public async Task DesbloquearAsync(int id, int idUsuarioAudit, CancellationToken ct)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id, ct);
        if (usuario == null) throw new KeyNotFoundException("Usuario no encontrado");

        usuario.Bloqueado = false;
        usuario.IntentosFallidos = 0;
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, usuario.IdUbicacion, "SEGURIDAD", "DESBLOQUEAR_CUENTA", "usuario", id.ToString(), $"Usuario desbloqueado: {usuario.NombreUsuario}", cancellationToken: ct);
    }

    public async Task DesactivarAsync(int id, int idUsuarioAudit, CancellationToken ct)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id, ct);
        if (usuario == null) throw new KeyNotFoundException("Usuario no encontrado");

        usuario.Activo = false;
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, usuario.IdUbicacion, "SEGURIDAD", "DESACTIVAR_USUARIO", "usuario", id.ToString(), $"Usuario desactivado: {usuario.NombreUsuario}", cancellationToken: ct);
    }

    public async Task CambiarContrasenaAsync(int id, CambiarContrasenaDto dto, int idUsuarioAudit, CancellationToken ct)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id, ct);
        if (usuario == null) throw new KeyNotFoundException("Usuario no encontrado");

        if (!_hasher.Verify(dto.ContrasenaActual, usuario.HashContrasena))
            throw new UnauthorizedAccessException("Contraseña actual incorrecta");

        usuario.HashContrasena = _hasher.Hash(dto.NuevaContrasena);
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, usuario.IdUbicacion, "SEGURIDAD", "CAMBIAR_CONTRASENA", "usuario", id.ToString(), "Contraseña cambiada", cancellationToken: ct);
    }

    public async Task IncrementarIntentosFallidosAsync(int idUsuario, CancellationToken ct)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE usuario SET intentos_fallidos = intentos_fallidos + 1 WHERE id_usuario = {idUsuario}", ct);
    }

    public async Task ResetearIntentosFallidosAsync(int idUsuario, CancellationToken ct)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE usuario SET intentos_fallidos = 0 WHERE id_usuario = {idUsuario}", ct);
    }

    public async Task ActualizarUltimoAccesoAsync(int idUsuario, CancellationToken ct)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE usuario SET ultimo_acceso = now() WHERE id_usuario = {idUsuario}", ct);
    }
}