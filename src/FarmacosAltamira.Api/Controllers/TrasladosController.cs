using FarmacosAltamira.Application.Traslados.Dto;
using FarmacosAltamira.Application.Traslados.Services;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Api.Extensions;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FarmacosAltamira.Api.Controllers;

[ApiController]
[Route("api/v1/traslados")]
[Authorize(Policy = "BODEGA:TRASLADO")]
public sealed class TrasladosController : ControllerBase
{
    private readonly TrasladoService _trasladoService;

    public TrasladosController(TrasladoService trasladoService)
    {
        _trasladoService = trasladoService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TrasladoDto>>> Solicitar([FromBody] TrasladoCreateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var traslado = await _trasladoService.SolicitarAsync(dto, userId, ct);
        return CreatedAtAction(nameof(Obtener), new { id = traslado.IdTraslado }, ApiResponse<TrasladoDto>.Ok(traslado));
    }

    [HttpPost("{id:int}/despachar")]
    public async Task<ActionResult<ApiResponse<TrasladoDto>>> Despachar(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var traslado = await _trasladoService.DespacharAsync(id, userId, ct);
        return Ok(ApiResponse<TrasladoDto>.Ok(traslado));
    }

    [HttpPost("{id:int}/recibir")]
    public async Task<ActionResult<ApiResponse<TrasladoDto>>> Recibir(int id, [FromBody] TrasladoRecibirDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var traslado = await _trasladoService.RecibirAsync(id, dto, userId, ct);
        return Ok(ApiResponse<TrasladoDto>.Ok(traslado));
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<ActionResult<ApiResponse<TrasladoDto>>> Cancelar(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var traslado = await _trasladoService.CancelarAsync(id, userId, ct);
        return Ok(ApiResponse<TrasladoDto>.Ok(traslado));
    }

    [HttpGet]
    [Authorize(Policy = "BODEGA:CONSULTAR")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<TrasladoDto>>>> Listar(
        [FromQuery] string? estado,
        [FromQuery] int? idOrigen,
        [FromQuery] int? idDestino,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var estadoEnum = Enum.TryParse<EstadoTraslado>(estado, true, out var e) ? e : (EstadoTraslado?)null;
        var result = await _trasladoService.ListarAsync(estadoEnum, idOrigen, idDestino, pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<TrasladoDto>>.Ok(result.ToContract()));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "BODEGA:CONSULTAR")]
    public async Task<ActionResult<ApiResponse<TrasladoDto>>> Obtener(int id, CancellationToken ct)
    {
        var traslado = await _trasladoService.ObtenerPorIdAsync(id, ct);
        return traslado == null
            ? NotFound(ApiResponse<TrasladoDto>.Fail("Traslado no encontrado", "NO_ENCONTRADO"))
            : Ok(ApiResponse<TrasladoDto>.Ok(traslado));
    }
}