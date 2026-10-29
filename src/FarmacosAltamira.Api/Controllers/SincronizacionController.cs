using FarmacosAltamira.Application.Catalogo.Dto;
using FarmacosAltamira.Application.Catalogo.Services;
using FarmacosAltamira.Application.Inventario.Dto;
using FarmacosAltamira.Application.Inventario.Services;
using FarmacosAltamira.Application.Seguridad.Dto;
using FarmacosAltamira.Application.Seguridad.Services;
using FarmacosAltamira.Application.Sincronizacion.Dto;
using FarmacosAltamira.Application.Sincronizacion.Services;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FarmacosAltamira.Api.Controllers;

[ApiController]
[Route("api/v1/sync")]
public sealed class SincronizacionController : ControllerBase
{
    private readonly ProductoService _productoService;
    private readonly LoteService _loteService;
    private readonly UsuarioService _usuarioService;
    private readonly ParametroService _parametroService;
    private readonly SincronizacionService _sincronizacionService;

    public SincronizacionController(
        ProductoService productoService,
        LoteService loteService,
        UsuarioService usuarioService,
        ParametroService parametroService,
        SincronizacionService sincronizacionService)
    {
        _productoService = productoService;
        _loteService = loteService;
        _usuarioService = usuarioService;
        _parametroService = parametroService;
        _sincronizacionService = sincronizacionService;
    }

    [HttpGet("catalogo")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SyncProductoDto>>>> Catalogo(
        [FromQuery] DateTimeOffset? desde,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 1000,
        CancellationToken ct = default)
    {
        var query = _productoService.ListarAsync(null, true, pagina, Math.Min(tamano, 2000), ct);
        // En implementación real, filtrar por actualizado_en >= desde
        var productos = (await query).Items;

        var resultado = productos.Select(p => new SyncProductoDto(
            p.IdProducto,
            p.CodigoBarras,
            p.NombreComercial,
            p.PrecioVenta,
            p.TasaIva,
            p.EsControlado,
            p.Activo)).ToList();

        return Ok(ApiResponse<IReadOnlyList<SyncProductoDto>>.Ok(resultado));
    }

    [HttpGet("promociones")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SyncPromocionDto>>>> Promociones(
        [FromQuery] DateTimeOffset? desde,
        CancellationToken ct = default)
    {
        // Implementar consulta de promociones vigentes
        return Ok(ApiResponse<IReadOnlyList<SyncPromocionDto>>.Ok(Array.Empty<SyncPromocionDto>()));
    }

    [HttpGet("usuarios")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SyncUsuarioDto>>>> Usuarios(
        [FromQuery] DateTimeOffset? desde,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 1000,
        CancellationToken ct = default)
    {
        var usuarios = await _usuarioService.ListarAsync(pagina, Math.Min(tamano, 2000), ct);
        
        var resultado = usuarios.Items.Select(u => new SyncUsuarioDto(
            u.IdUsuario,
            u.NombreUsuario,
            null, // HashContrasena no se sincroniza por seguridad
            u.Rol,
            u.Activo,
            u.Bloqueado,
            u.UltimoAcceso ?? DateTimeOffset.MinValue)).ToList();

        return Ok(ApiResponse<IReadOnlyList<SyncUsuarioDto>>.Ok(resultado));
    }

    [HttpGet("existencias")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SyncExistenciaDto>>>> Existencias(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 1000,
        CancellationToken ct = default)
    {
        // La ubicación se determina por la estación autenticada
        var idUbicacion = int.Parse(User.FindFirst("ubi")?.Value ?? "0");
        if (idUbicacion == 0)
            return BadRequest(ApiResponse<IReadOnlyList<SyncExistenciaDto>>.Fail("Ubicación no determinada", "UBICACION_REQUERIDA"));

        var existencias = await _loteService.ConsultarExistenciasAsync(null, idUbicacion, pagina, Math.Min(tamano, 2000), ct);
        
        var resultado = existencias.Items.Select(e => new SyncExistenciaDto(
            e.IdLote,
            e.Cantidad,
            e.CantidadReservada)).ToList();

        return Ok(ApiResponse<IReadOnlyList<SyncExistenciaDto>>.Ok(resultado));
    }

    [HttpGet("bajar")]
    public async Task<ActionResult<ApiResponse<BajarDatosResultado>>> Bajar(
        [FromQuery] DateTimeOffset? desde,
        CancellationToken ct = default)
    {
        var resultado = await _sincronizacionService.BajarDatosAsync(desde, ct);
        return Ok(ApiResponse<BajarDatosResultado>.Ok(resultado));
    }

    [HttpGet("parametros")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SyncParametroDto>>>> Parametros(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 500,
        CancellationToken ct = default)
    {
        var parametros = await _parametroService.ListarAsync(pagina, Math.Min(tamano, 1000), ct);
        
        var resultado = parametros.Items.Select(p => new SyncParametroDto(
            p.Clave,
            p.Valor)).ToList();

        return Ok(ApiResponse<IReadOnlyList<SyncParametroDto>>.Ok(resultado));
    }

    [HttpPost("subir")]
    [EnableRateLimiting("sync")]
    public async Task<ActionResult<ApiResponse<SubirVentasResponse>>> SubirVentas(CancellationToken ct)
    {
        var resultado = await _sincronizacionService.SubirVentasPendientesAsync(ct);
        
        var response = new SubirVentasResponse(
            resultado.Exitosas,
            resultado.Fallidas,
            resultado.Total,
            resultado.Exitosas > 0 ? "Sincronización completada" : "No hay ventas pendientes o todas fallaron");

        return Ok(ApiResponse<SubirVentasResponse>.Ok(response));
    }
}

public sealed record SubirVentasResponse(
    int Exitosas,
    int Fallidas,
    int Total,
    string Mensaje);