using FarmacosAltamira.Application.Caja.Dto;
using FarmacosAltamira.Application.Caja.Services;
using FarmacosAltamira.Application.Ventas.Services;
using FarmacosAltamira.Api.Extensions;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FarmacosAltamira.Api.Controllers;

[ApiController]
[Route("api/v1/cortes")]
[Authorize(Policy = "POS:CORTE")]
public sealed class CortesController : ControllerBase
{
    private readonly CorteService _corteService;

    public CortesController(CorteService corteService)
    {
        _corteService = corteService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CorteDto>>> Abrir([FromBody] CorteAperturaDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var corte = await _corteService.AbrirAsync(dto, userId, ct);
        return Ok(ApiResponse<CorteDto>.Ok(corte));
    }

    [HttpPost("{id:int}/cierre")]
    public async Task<ActionResult<ApiResponse<CorteDto>>> Cerrar(int id, [FromBody] CorteCierreDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var corte = await _corteService.CerrarAsync(id, dto, userId, ct);
        return Ok(ApiResponse<CorteDto>.Ok(corte));
    }
}

[ApiController]
[Route("api/v1/ventas")]
[Authorize(Policy = "POS:REGISTRAR_VENTA")]
public sealed class VentasController : ControllerBase
{
    private readonly VentaService _ventaService;

    public VentasController(VentaService ventaService)
    {
        _ventaService = ventaService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VentaResultadoDto>>> Registrar([FromBody] VentaCreateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var resultado = await _ventaService.RegistrarAsync(dto, userId, ct);
        
        var statusCode = resultado.Resultado == "CREADA" ? 201 : 200;
        return StatusCode(statusCode, ApiResponse<VentaResultadoDto>.Ok(resultado));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<VentaDto>>>> Listar(
        [FromQuery] int? idCorte,
        [FromQuery] DateTimeOffset? desde,
        [FromQuery] DateTimeOffset? hasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var result = await _ventaService.ListarAsync(idCorte, desde, hasta, pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<VentaDto>>.Ok(result.ToContract()));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "POS:CONSULTAR")]
    public async Task<ActionResult<ApiResponse<VentaDto>>> Obtener(int id, CancellationToken ct)
    {
        var venta = await _ventaService.ObtenerPorIdAsync(id, ct);
        return venta == null
            ? NotFound(ApiResponse<VentaDto>.Fail("Venta no encontrada", "NO_ENCONTRADO"))
            : Ok(ApiResponse<VentaDto>.Ok(venta));
    }

    [HttpPost("{id:int}/devoluciones")]
    [Authorize(Policy = "POS:DEVOLUCION")]
    public async Task<ActionResult<ApiResponse<DevolucionDto>>> Devolver(int id, [FromBody] DevolucionCreateDto dto, CancellationToken ct)
    {
        if (dto.IdVenta != id) return BadRequest(ApiResponse<DevolucionDto>.Fail("El ID de la venta no coincide", "DATOS_INVALIDOS"));
        
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var devolucion = await _ventaService.RegistrarDevolucionAsync(dto, userId, ct);
        return Ok(ApiResponse<DevolucionDto>.Ok(devolucion));
    }

    [HttpPost("{id:int}/cancelacion")]
    [Authorize(Policy = "POS:CANCELAR")]
    public async Task<ActionResult<ApiResponse<CancelacionDto>>> Cancelar(int id, [FromBody] CancelacionCreateDto dto, CancellationToken ct)
    {
        if (dto.IdVenta != id) return BadRequest(ApiResponse<CancelacionDto>.Fail("El ID de la venta no coincide", "DATOS_INVALIDOS"));
        
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var cancelacion = await _ventaService.CancelarAsync(dto, userId, ct);
        return Ok(ApiResponse<CancelacionDto>.Ok(cancelacion));
    }
}