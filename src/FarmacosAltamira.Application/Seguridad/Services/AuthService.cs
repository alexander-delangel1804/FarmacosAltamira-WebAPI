using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Seguridad.Services;

public sealed class AuthService
{
    private readonly ICentralDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly IEstacionService _estacion;
    private readonly IAuditoria _auditoria;
    private readonly ParametroService _parametros;
    private readonly UsuarioService _usuarios;

    public AuthService(
        ICentralDbContext db,
        IPasswordHasher hasher,
        IJwtTokenService jwt,
        IEstacionService estacion,
        IAuditoria auditoria,
        ParametroService parametros,
        UsuarioService usuarios)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _estacion = estacion;
        _auditoria = auditoria;
        _parametros = parametros;
        _usuarios = usuarios;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string ipOrigen, CancellationToken ct)
    {
        // Validar estación
        var estacionValida = await _estacion.ValidarEstacionAsync(request.IdEstacion, request.SecretoEstacion, ct);
        if (!estacionValida)
        {
            await _auditoria.RegistrarAsync(0, null, "SEGURIDAD", "INICIO_SESION_FALLIDO", "estacion", request.IdEstacion.ToString(), "Credenciales de estación inválidas", ipOrigen, cancellationToken: ct);
            throw new UnauthorizedAccessException("Credenciales de estación inválidas");
        }

        // Buscar usuario
        var usuario = await _db.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Ubicacion)
            .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario && u.Activo, ct);

        if (usuario == null || !_hasher.Verify(request.Contrasena, usuario.HashContrasena))
        {
            await _auditoria.RegistrarAsync(0, null, "SEGURIDAD", "INICIO_SESION_FALLIDO", "usuario", request.NombreUsuario, "Usuario o contraseña incorrectos", ipOrigen, cancellationToken: ct);

            if (usuario != null)
            {
                await _usuarios.IncrementarIntentosFallidosAsync(usuario.IdUsuario, ct);

                var maxIntentos = await _parametros.ObtenerEnteroAsync("MAX_INTENTOS_LOGIN", 5, ct);
                if (usuario.IntentosFallidos + 1 >= maxIntentos)
                {
                    usuario.Bloqueado = true;
                    await _db.SaveChangesAsync(ct);
                    await _auditoria.RegistrarAsync(0, null, "SEGURIDAD", "BLOQUEO_CUENTA", "usuario", usuario.IdUsuario.ToString(), $"Cuenta bloqueada tras {maxIntentos} intentos fallidos", ipOrigen, cancellationToken: ct);
                }
            }

            throw new UnauthorizedAccessException("Usuario o contraseña incorrectos");
        }

        if (usuario.Bloqueado)
        {
            await _auditoria.RegistrarAsync(usuario.IdUsuario, usuario.IdUbicacion, "SEGURIDAD", "INICIO_SESION_FALLIDO", "usuario", usuario.IdUsuario.ToString(), "Cuenta bloqueada", ipOrigen, cancellationToken: ct);
            throw new UnauthorizedAccessException("Cuenta bloqueada");
        }

        // Validar que la estación corresponde a la ubicación del usuario (si no es admin global)
        var idUbicacionEstacion = await _estacion.ObtenerIdUbicacionPorEstacionAsync(request.IdEstacion, ct);
        var idCajaEstacion = await _estacion.ObtenerIdCajaPorEstacionAsync(request.IdEstacion, ct);

        if (usuario.IdUbicacion.HasValue && idUbicacionEstacion.HasValue && usuario.IdUbicacion != idUbicacionEstacion)
        {
            await _auditoria.RegistrarAsync(usuario.IdUsuario, usuario.IdUbicacion, "SEGURIDAD", "INICIO_SESION_FALLIDO", "usuario", usuario.IdUsuario.ToString(), "La estación no corresponde a la ubicación del usuario", ipOrigen, cancellationToken: ct);
            throw new UnauthorizedAccessException("La estación no corresponde a la ubicación del usuario");
        }

        // Resetear intentos fallidos
        await _usuarios.ResetearIntentosFallidosAsync(usuario.IdUsuario, ct);
        await _usuarios.ActualizarUltimoAccesoAsync(usuario.IdUsuario, ct);

        // Obtener permisos del rol
        var permisos = await _db.Roles
            .AsNoTracking()
            .Where(r => r.IdRol == usuario.IdRol)
            .SelectMany(r => r.Permisos)
            .Select(p => $"{p.Modulo}:{p.Accion}")
            .ToListAsync(ct);

        var accessToken = _jwt.GenerateAccessToken(usuario.IdUsuario, usuario.Rol.Nombre, usuario.IdUbicacion, permisos);
        var refreshToken = _jwt.GenerateRefreshToken();

        // Guardar refresh token hash
        var hashRefresh = _hasher.Hash(refreshToken);
        var sesion = new SesionUsuario
        {
            IdUsuario = usuario.IdUsuario,
            HashToken = hashRefresh,
            Expiracion = DateTimeOffset.UtcNow.AddDays(7),
            Revocado = false
        };
        _db.SesionesUsuario.Add(sesion);
        await _db.SaveChangesAsync(ct);

        // Actualizar última conexión de la estación
        await _estacion.ActualizarUltimaConexionAsync(request.IdEstacion, "1.0.0", ct);

        await _auditoria.RegistrarAsync(usuario.IdUsuario, usuario.IdUbicacion, "SEGURIDAD", "INICIO_SESION", "usuario", usuario.IdUsuario.ToString(), "Inicio de sesión exitoso", ipOrigen, cancellationToken: ct);

        return new LoginResponse(
            accessToken,
            refreshToken,
            usuario.IdUsuario,
            usuario.NombreUsuario,
            usuario.NombreCompleto,
            usuario.Rol.Nombre,
            usuario.IdUbicacion,
            usuario.Ubicacion?.Nombre,
            idCajaEstacion,
            idCajaEstacion.HasValue ? idCajaEstacion.Value.ToString() : null,
            DateTimeOffset.UtcNow.AddMinutes(30));
    }

    public async Task CerrarSesionAsync(int idUsuario, string? jti, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(jti))
        {
            var sesion = await _db.SesionesUsuario
                .FirstOrDefaultAsync(s => s.IdUsuario == idUsuario && s.HashToken == jti && !s.Revocado, ct);

            if (sesion != null)
            {
                sesion.Revocado = true;
                await _db.SaveChangesAsync(ct);
            }
        }

        await _auditoria.RegistrarAsync(idUsuario, null, "SEGURIDAD", "CIERRE_SESION", "usuario", idUsuario.ToString(), "Cierre de sesión", cancellationToken: ct);
    }

    public async Task<bool> ValidarRefreshTokenAsync(int idUsuario, string refreshToken, CancellationToken ct)
    {
        var hash = _hasher.Hash(refreshToken);
        return await _db.SesionesUsuario
            .AnyAsync(s => s.IdUsuario == idUsuario && s.HashToken == hash && !s.Revocado && s.Expiracion > DateTimeOffset.UtcNow, ct);
    }

    public async Task RevocarRefreshTokenAsync(int idUsuario, string refreshToken, CancellationToken ct)
    {
        var hash = _hasher.Hash(refreshToken);
        var sesion = await _db.SesionesUsuario
            .FirstOrDefaultAsync(s => s.IdUsuario == idUsuario && s.HashToken == hash && !s.Revocado, ct);

        if (sesion != null)
        {
            sesion.Revocado = true;
            await _db.SaveChangesAsync(ct);
        }
    }
}