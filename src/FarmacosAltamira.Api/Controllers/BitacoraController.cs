using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Application.Seguridad.Services;
using FarmacosAltamira.Api.Extensions;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FarmacosAltamira.Api.Controllers;

[ApiController]
[Route("api/v1/bitacora")]
[Authorize(Policy = "SEGURIDAD:BITACORA")]
public sealed class BitacoraController : ControllerBase
{
    private readonly BitacoraService _bitacoraService;

    public BitacoraController(BitacoraService bitacoraService)
    {
        _bitacoraService = bitacoraService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<BitacoraDto>>>> Consultar(
        [FromQuery] int? idUsuario,
        [FromQuery] int? idUbicacion,
        [FromQuery] string? modulo,
        [FromQuery] string? accion,
        [FromQuery] DateTimeOffset? fechaDesde,
        [FromQuery] DateTimeOffset? fechaHasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var filtro = new BitacoraFiltroDto
        {
            IdUsuario = idUsuario,
            IdUbicacion = idUbicacion,
            Modulo = modulo,
            Accion = accion,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Pagina = pagina,
            TamanoPagina = tamano
        };

        var result = await _bitacoraService.ConsultarAsync(filtro, ct);
        return Ok(ApiResponse<PaginatedResponse<BitacoraDto>>.Ok(result.ToContract()));
    }
}