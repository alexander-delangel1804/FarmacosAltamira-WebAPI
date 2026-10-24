using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Application.Seguridad.Services;
using FarmacosAltamira.Api.Extensions;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FarmacosAltamira.Api.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize(Policy = "SEGURIDAD:USUARIOS")]
public sealed class UsuariosController : ControllerBase
{
    private readonly UsuarioService _usuarioService;

    public UsuariosController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<UsuarioDto>>>> Listar([FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default)
    {
        var result = await _usuarioService.ListarAsync(pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<UsuarioDto>>.Ok(result.ToContract()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> Obtener(int id, CancellationToken ct)
    {
        var usuario = await _usuarioService.ObtenerPorIdAsync(id, ct);
        return usuario == null
            ? NotFound(ApiResponse<UsuarioDto>.Fail("Usuario no encontrado", "NO_ENCONTRADO"))
            : Ok(ApiResponse<UsuarioDto>.Ok(usuario));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> Crear([FromBody] UsuarioCreateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var usuario = await _usuarioService.CrearAsync(dto, userId, ct);
        return CreatedAtAction(nameof(Obtener), new { id = usuario.IdUsuario }, ApiResponse<UsuarioDto>.Ok(usuario));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> Actualizar(int id, [FromBody] UsuarioUpdateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var usuario = await _usuarioService.ActualizarAsync(id, dto, userId, ct);
        return Ok(ApiResponse<UsuarioDto>.Ok(usuario));
    }

    [HttpPost("{id:int}/bloquear")]
    public async Task<ActionResult<ApiResponse<object>>> Bloquear(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        await _usuarioService.BloquearAsync(id, userId, ct);
        return Ok(ApiResponse<object>.Ok(null, "Usuario bloqueado"));
    }

    [HttpPost("{id:int}/desbloquear")]
    public async Task<ActionResult<ApiResponse<object>>> Desbloquear(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        await _usuarioService.DesbloquearAsync(id, userId, ct);
        return Ok(ApiResponse<object>.Ok(null, "Usuario desbloqueado"));
    }

    [HttpPost("{id:int}/desactivar")]
    public async Task<ActionResult<ApiResponse<object>>> Desactivar(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        await _usuarioService.DesactivarAsync(id, userId, ct);
        return Ok(ApiResponse<object>.Ok(null, "Usuario desactivado"));
    }
}