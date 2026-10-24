using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Application.Seguridad.Services;
using FarmacosAltamira.Api.Extensions;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FarmacosAltamira.Api.Controllers;

[ApiController]
[Route("api/v1/parametros")]
[Authorize(Policy = "SEGURIDAD:PARAMETROS")]
public sealed class ParametrosController : ControllerBase
{
    private readonly ParametroService _parametroService;

    public ParametrosController(ParametroService parametroService)
    {
        _parametroService = parametroService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ParametroDto>>>> Listar([FromQuery] int pagina = 1, [FromQuery] int tamano = 50, CancellationToken ct = default)
    {
        var result = await _parametroService.ListarAsync(pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<ParametroDto>>.Ok(result.ToContract()));
    }

    [HttpGet("{clave}")]
    public async Task<ActionResult<ApiResponse<ParametroDto>>> Obtener(string clave, CancellationToken ct)
    {
        var param = await _parametroService.ObtenerAsync(clave, ct);
        return param == null
            ? NotFound(ApiResponse<ParametroDto>.Fail("Parámetro no encontrado", "NO_ENCONTRADO"))
            : Ok(ApiResponse<ParametroDto>.Ok(param));
    }

    [HttpPut("{clave}")]
    public async Task<ActionResult<ApiResponse<object>>> Actualizar(string clave, [FromBody] ParametroUpdateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        await _parametroService.ActualizarAsync(clave, dto, userId, ct);
        return Ok(ApiResponse<object>.Ok(null, "Parámetro actualizado"));
    }
}