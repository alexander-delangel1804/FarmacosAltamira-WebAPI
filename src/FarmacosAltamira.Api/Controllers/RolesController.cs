using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Application.Seguridad.Services;
using FarmacosAltamira.Api.Extensions;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FarmacosAltamira.Api.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize(Policy = "SEGURIDAD:USUARIOS")]
public sealed class RolesController : ControllerBase
{
    private readonly RolService _rolService;

    public RolesController(RolService rolService)
    {
        _rolService = rolService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<RolDto>>>> Listar([FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default)
    {
        var result = await _rolService.ListarAsync(pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<RolDto>>.Ok(result.ToContract()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<RolDto>>> Obtener(int id, CancellationToken ct)
    {
        var rol = await _rolService.ObtenerPorIdAsync(id, ct);
        return rol == null
            ? NotFound(ApiResponse<RolDto>.Fail("Rol no encontrado", "NO_ENCONTRADO"))
            : Ok(ApiResponse<RolDto>.Ok(rol));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RolDto>>> Crear([FromBody] RolCreateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var rol = await _rolService.CrearAsync(dto, userId, ct);
        return CreatedAtAction(nameof(Obtener), new { id = rol.IdRol }, ApiResponse<RolDto>.Ok(rol));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<RolDto>>> Actualizar(int id, [FromBody] RolUpdateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var rol = await _rolService.ActualizarAsync(id, dto, userId, ct);
        return Ok(ApiResponse<RolDto>.Ok(rol));
    }

    [HttpGet("permisos")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PermisoDto>>>> ListarPermisos([FromQuery] int pagina = 1, [FromQuery] int tamano = 50, CancellationToken ct = default)
    {
        var result = await _rolService.ListarPermisosAsync(pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<PermisoDto>>.Ok(result.ToContract()));
    }
}