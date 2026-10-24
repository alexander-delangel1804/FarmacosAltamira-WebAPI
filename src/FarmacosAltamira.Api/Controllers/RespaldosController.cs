using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Application.Seguridad.Services;
using FarmacosAltamira.Api.Extensions;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FarmacosAltamira.Api.Controllers;

[ApiController]
[Route("api/v1/respaldos")]
[Authorize(Policy = "SEGURIDAD:RESPALDOS")]
public sealed class RespaldosController : ControllerBase
{
    private readonly RespaldoService _respaldoService;

    public RespaldosController(RespaldoService respaldoService)
    {
        _respaldoService = respaldoService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<RespaldoDto>>>> Listar(
        [FromQuery] int? idUsuario,
        [FromQuery] string? tipo,
        [FromQuery] string? estado,
        [FromQuery] DateTimeOffset? fechaDesde,
        [FromQuery] DateTimeOffset? fechaHasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var filtro = new RespaldoFiltroDto
        {
            IdUsuario = idUsuario,
            Tipo = tipo,
            Estado = estado,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Pagina = pagina,
            TamanoPagina = tamano
        };

        var result = await _respaldoService.ListarAsync(filtro, ct);
        return Ok(ApiResponse<PaginatedResponse<RespaldoDto>>.Ok(result.ToContract()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RespaldoDto>>> Iniciar([FromBody] IniciarRespaldoRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var respaldo = await _respaldoService.RegistrarInicioAsync(userId, request.Tipo, request.Ruta, ct);
        return Ok(ApiResponse<RespaldoDto>.Ok(respaldo));
    }

    [HttpPost("{id:int}/finalizar")]
    public async Task<ActionResult<ApiResponse<object>>> Finalizar(int id, [FromBody] FinalizarRespaldoRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        await _respaldoService.FinalizarAsync(id, request.Exitoso, request.TamanoMb, userId, ct);
        return Ok(ApiResponse<object>.Ok(null, "Respaldo finalizado"));
    }

    [HttpPost("{id:int}/verificar")]
    public async Task<ActionResult<ApiResponse<object>>> Verificar(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        await _respaldoService.MarcarVerificadoAsync(id, userId, ct);
        return Ok(ApiResponse<object>.Ok(null, "Respaldo verificado"));
    }
}

public sealed record IniciarRespaldoRequest(string Tipo, string Ruta);
public sealed record FinalizarRespaldoRequest(bool Exitoso, decimal? TamanoMb);