using FarmacosAltamira.Application.Inventario.Dto;
using FarmacosAltamira.Application.Inventario.Services;
using FarmacosAltamira.Api.Extensions;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FarmacosAltamira.Api.Controllers;

[ApiController]
[Route("api/v1/inventario")]
[Authorize(Policy = "BODEGA:CONSULTAR")]
public sealed class InventarioController : ControllerBase
{
    private readonly LoteService _loteService;

    public InventarioController(LoteService loteService)
    {
        _loteService = loteService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ExistenciaDto>>>> Consultar(
        [FromQuery] int? idProducto,
        [FromQuery] int? idUbicacion,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var result = await _loteService.ConsultarExistenciasAsync(idProducto, idUbicacion, pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<ExistenciaDto>>.Ok(result.ToContract()));
    }

    [HttpGet("productos/{idProducto:int}/existencias")]
    [Authorize(Policy = "POS:CONSULTAR_STOCK")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ExistenciaDto>>>> ConsultarPorProducto(
        int idProducto,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var result = await _loteService.ConsultarExistenciasAsync(idProducto, null, pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<ExistenciaDto>>.Ok(result.ToContract()));
    }
}

[ApiController]
[Route("api/v1/lotes")]
[Authorize(Policy = "BODEGA:CONSULTAR")]
public sealed class LotesController : ControllerBase
{
    private readonly LoteService _loteService;

    public LotesController(LoteService loteService)
    {
        _loteService = loteService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<LoteDto>>>> Listar(
        [FromQuery] int? idProducto,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var result = await _loteService.ListarAsync(idProducto, pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<LoteDto>>.Ok(result.ToContract()));
    }
}

[ApiController]
[Route("api/v1/recepciones")]
[Authorize(Policy = "BODEGA:RECEPCION")]
public sealed class RecepcionesController : ControllerBase
{
    private readonly LoteService _loteService;

    public RecepcionesController(LoteService loteService)
    {
        _loteService = loteService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<RecepcionDto>>> Obtener(int id, CancellationToken ct)
    {
        var recepcion = await _loteService.ObtenerRecepcionPorIdAsync(id, ct);
        return recepcion == null
            ? NotFound(ApiResponse<RecepcionDto>.Fail("Recepción no encontrada", "NO_ENCONTRADO"))
            : Ok(ApiResponse<RecepcionDto>.Ok(recepcion));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RecepcionDto>>> Crear([FromBody] RecepcionCreateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var recepcion = await _loteService.RegistrarRecepcionAsync(dto, userId, ct);
        return CreatedAtAction(nameof(Obtener), new { id = recepcion.IdRecepcion }, ApiResponse<RecepcionDto>.Ok(recepcion));
    }
}

[ApiController]
[Route("api/v1/ajustes")]
[Authorize(Policy = "BODEGA:AJUSTE")]
public sealed class AjustesController : ControllerBase
{
    private readonly LoteService _loteService;

    public AjustesController(LoteService loteService)
    {
        _loteService = loteService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AjusteCreateDto>>> Registrar([FromBody] AjusteCreateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var ajuste = await _loteService.RegistrarAjusteAsync(dto, userId, ct);
        return Ok(ApiResponse<AjusteCreateDto>.Ok(ajuste));
    }
}

[ApiController]
[Route("api/v1/movimientos")]
[Authorize(Policy = "BODEGA:CONSULTAR")]
public sealed class MovimientosController : ControllerBase
{
    private readonly LoteService _loteService;

    public MovimientosController(LoteService loteService)
    {
        _loteService = loteService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<MovimientoDto>>>> Consultar(
        [FromQuery] int? idLote,
        [FromQuery] int? idUbicacion,
        [FromQuery] DateTimeOffset? desde,
        [FromQuery] DateTimeOffset? hasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var result = await _loteService.ConsultarMovimientosAsync(idLote, idUbicacion, desde, hasta, pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<MovimientoDto>>.Ok(result.ToContract()));
    }
}

[ApiController]
[Route("api/v1/alertas")]
[Authorize(Policy = "BODEGA:CONSULTAR")]
public sealed class AlertasController : ControllerBase
{
    private readonly LoteService _loteService;

    public AlertasController(LoteService loteService)
    {
        _loteService = loteService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<AlertaDto>>>> Listar(
        [FromQuery] int? idUbicacion,
        [FromQuery] bool soloNoAtendidas = true,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var result = await _loteService.ListarAlertasAsync(idUbicacion, soloNoAtendidas, pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<AlertaDto>>.Ok(result.ToContract()));
    }

    [HttpPost("{id:int}/atender")]
    public async Task<ActionResult<ApiResponse<object>>> Atender(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        await _loteService.MarcarAlertaAtendidaAsync(id, userId, ct);
        return Ok(ApiResponse<object>.Ok(null, "Alerta atendida"));
    }
}