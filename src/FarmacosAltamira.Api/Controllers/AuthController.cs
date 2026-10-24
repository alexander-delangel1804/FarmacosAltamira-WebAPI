using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Application.Seguridad.Services;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FarmacosAltamira.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("login")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var response = await _authService.LoginAsync(request, ip, ct);
        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var jti = User.FindFirst("jti")?.Value;
        await _authService.CerrarSesionAsync(userId, jti, ct);
        return Ok(ApiResponse<object>.Ok(null, "Sesión cerrada"));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var isValid = await _authService.ValidarRefreshTokenAsync(userId, request.RefreshToken, ct);
        
        if (!isValid)
            return Unauthorized(ApiResponse<LoginResponse>.Fail("Refresh token inválido o expirado", "TOKEN_EXPIRADO"));

        await _authService.RevocarRefreshTokenAsync(userId, request.RefreshToken, ct);

        // Generar nuevos tokens (requiere obtener usuario y permisos de nuevo)
        // Por simplicidad, en producción se implementaría un endpoint dedicado
        return BadRequest(ApiResponse<LoginResponse>.Fail("Implementar renovación completa", "NO_IMPLEMENTADO"));
    }

    [HttpPost("cambiar-contrasena")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> CambiarContrasena([FromBody] CambiarContrasenaDto request, [FromServices] UsuarioService usuarioService, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        await usuarioService.CambiarContrasenaAsync(userId, request, userId, ct);
        return Ok(ApiResponse<object>.Ok(null, "Contraseña cambiada"));
    }
}

public sealed record RefreshRequest(string RefreshToken);